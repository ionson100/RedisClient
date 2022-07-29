using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisRPC
{
    class InnerPerformerWorker
    {
        public async Task Perform(ISubscriber subscriber, RedisChannel channel,
            Func<RedisChannel, RedisValue,string> handler)
        {
            await subscriber.SubscribeAsync(channel, async (channelE, message) =>
            {
                try
                {
                    _ = Task.Run(async () =>
                    {
                        string sd = message.ToString();
                        MyClass m = JsonConvert.DeserializeObject<MyClass>(message);
                        string res = handler.Invoke(channel, m.param);
                        await subscriber.PublishAsync($"{channel}:{m.key}", res);
                    });
                    
                }
                catch (Exception e)
                {
                    throw new InnerExceptionRpc($"Произошло при исполениие процедуры: {e.Message}");
                }
              
            });
        }
    }

    class MyClass
    {
        public string param { get; set; }
        public string key { get; set; }
    }
}
