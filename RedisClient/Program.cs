using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedisRPC;
using StackExchange.Redis;

namespace RedisClient
{
    internal class Program
    {
        private static int hotel = 100;

        private static async Task Main(string[] args)
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions //45.8.145.232
            {
                Password = Auth.Pwd, EndPoints = {$"{Auth.Host}:6379"}, AbortOnConnectFail = false
            });
            Console.WriteLine($" hotel:{hotel} printer:{1}");
            Console.WriteLine($" host: {Auth.Host}");

            var sub = redis.GetSubscriber();
            await sub.PerformerRpcAsync($"canel:{hotel}:{1}",
                (channel, value) =>
                {
                    Console.WriteLine(value);
                    return JsonConvert.SerializeObject(new AnswerPrinter {IsOk = false, ErrorMessage = "Закончилась бумага"});
                });
          
            Console.Read();
            await sub.UnsubscribeAsync($"canel:{hotel}:{1}");
        }

        

        public class AnswerPrinter
        {
            public bool IsOk { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}