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
                    string sd = message.ToString();
                    Dictionary<string,string> m = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                    string  res=  handler.Invoke(channel,m["param"]);
                    await subscriber.PublishAsync($"{channel}:{m["key"]}", res);
                }
                catch (Exception e)
                {
                    throw new InnerExceptionRpc($"Произошло при сполениие процедуры: {e.Message}");
                }
              
            });
        }
    }
}
