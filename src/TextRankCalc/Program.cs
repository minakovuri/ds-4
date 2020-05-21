using System;
using System.Text;
using System.Collections.Generic;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using TextRankCalc.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace TextRankCalc
{
    class Program
    {
        private static bool running = true;

        private static readonly string _natsUrl = "nats://localhost:4222";
        //private static readonly string _natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");

        private static readonly string _redisUrl = "localhost:6379";
        //private static readonly string _redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");

        static void Main(string[] args)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_redisUrl);
            IConnection nats = new ConnectionFactory().CreateConnection(_natsUrl);
            
            Program.Subscribe();

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                Program.running = false;
            };

            while (running) { }
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

                Console.WriteLine($"Start calculating rank for {id}");

                var model = JsonSerializer.Deserialize<RedisPayload>(serialisedData);

                double rank = CalculateTextRank(model.Data);

                Console.WriteLine($"Rank of {id}: {rank}");

                model.Rank = rank;

                var payload = JsonSerializer.Serialize(model);
                db.StringSet(id, payload);
            });
        }

        static double CalculateTextRank(string text)
        {
            var vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u' };

            var consonants = new HashSet<char> { 'q', 'w', 'r', 't', 'y', 'p', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };

            string lowerCaseText = text.ToLower();

            int vowelsCount = lowerCaseText.Count(ch => vowels.Contains(ch));
            int consonantsCount = lowerCaseText.Count(ch => consonants.Contains(ch));

            if (consonantsCount == 0)
            {
                consonantsCount = 1;
            }

            return vowelsCount / consonantsCount;
        }
    }
}
