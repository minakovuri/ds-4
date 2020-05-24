using System;
using System.Text;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using JobLogger.Models;
using StackExchange.Redis;
using System.Text.Json;
//using System.Text.Json.Serialization;

namespace JobLogger
{
    class Program
    {   
        private static bool running = true;

        private static readonly string _natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");

        private static readonly string _redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");

        static void Main(string[] args)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_redisUrl);
            IConnection nats = new ConnectionFactory().CreateConnection(_natsUrl);
            
            Program.Subscribe();

            Console.WriteLine("----- JobLogger service is running ------");

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                Program.running = false;
            };

            while (running) { }

            Console.WriteLine("------ JobLogger service is shut down ------ ");
        }

        static void Subscribe()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_redisUrl);
            IConnection nats = new ConnectionFactory().CreateConnection(_natsUrl);

            var events = nats.Observe("events")
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            events.Subscribe(msg =>
            {
                IDatabase db = redis.GetDatabase();

                string id = msg.Split('|').Last();
                string serialisedData = db.StringGet(id);

                var model = JsonSerializer.Deserialize<RedisPayload>(serialisedData);

                Console.WriteLine("ID: " + id + ", description: " + model.Description + ", data: " + model.Data);
            });
        }
    }
}
