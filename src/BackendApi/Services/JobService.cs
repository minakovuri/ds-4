using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using StackExchange.Redis;
using Grpc.Core;
using NATS.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BackendApi.Models;
using System.Text.Json.Serialization;

namespace BackendApi.Services
{
    public class JobService : Job.JobBase
    {
        private static int MAX_RETRIES = 15;
        private static int SLEEP_TIMEOUT = 500;

        private readonly static Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private readonly ILogger<JobService> _logger;
        private readonly ConnectionMultiplexer _redis;

        private readonly IConnection _nats;

        private readonly string _natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");

        private readonly string _redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
            _redis = ConnectionMultiplexer.Connect(_redisUrl);
            _nats = new ConnectionFactory().CreateConnection(_natsUrl);
        }

        async public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse
            {
                Id = id
            };
            _jobs[id] = request.Description;

            await SendMessageToRedis(id, request.Description, request.Data);

            SendMessageToNats(id);

            return await Task.FromResult(resp);
        }

        public override Task<GetProcessingResultResponse> GetProcessingResult(GetProcessingResultRequest request, ServerCallContext context)
        {
            RedisPayload payload = GetPayloadFromRedis(request.Id);

            var resp = new GetProcessingResultResponse { 
                Rank = payload.Rank,
                Status = payload.Rank == -1 
                    ? "1"
                    : "2",
                Description = payload.Description,
                Text = payload.Data,
            };
    
            return Task.FromResult(resp);
        }
        
        private RedisPayload GetPayloadFromRedis(string id) 
        {
            IDatabase db = _redis.GetDatabase();
            int index = 0;
            while (index++ < MAX_RETRIES)
            {
                string JSON = db.StringGet(id);

                var model = JsonSerializer.Deserialize<RedisPayload>(JSON);
                if (model.Rank != -1)
                {
                    return model;
                }

                Thread.Sleep(SLEEP_TIMEOUT);
            }

            return null;
        }

        async private Task SendMessageToRedis(string id, string description, string data)
        {
            IDatabase db = _redis.GetDatabase();

            string serializedData = JsonSerializer.Serialize(new RedisPayload{
                Description = description,
                Data = data,
            });

            await db.StringSetAsync(id, serializedData);
        }

        private void SendMessageToNats(string id)
        {
            string message = $"JobCreated|{id}";
            byte[] payload = Encoding.Default.GetBytes(message);

           _nats.Publish("events", payload);
        }
    }
}