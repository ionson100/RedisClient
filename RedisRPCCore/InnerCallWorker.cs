using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisRPC
{
    class InnerCallWorker
    {
        private readonly ISubscriber _subscriber;
        private readonly RedisChannel _channel;
        private readonly CommandFlags _flags;


        public InnerCallWorker(ISubscriber subscriber, RedisChannel channel, CommandFlags flags)
        {
            _subscriber = subscriber;
            _channel = channel;
            _flags = flags;
        }

        public async Task<string> Call(string param,int timeOut)
        {
            TaskWrapper wrapper = new TaskWrapper {Task = new Task(() => { })};
          
            IConnectionMultiplexer mp = _subscriber.Multiplexer;

            var ee = InnerCall(param, wrapper);
            ee.Wait();
            if (wrapper.Exception != null)
            {
                wrapper.Action?.Invoke();
                throw wrapper.Exception;
            }
            else
            {
                if (wrapper.Task.Wait(timeOut))
                {
                    var tt = wrapper.ResultRedisValue;
                    return tt;
                }
                else
                {
                    wrapper.Action?.Invoke();
                   throw new InnerExceptionRpc("Превышен лимит времени ожидания ответа от принтера");
                }
            }

        }

        private async Task InnerCall( string param,TaskWrapper wrapper)
        {
            string key = Utils.RandomString(12);
            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                {"param", param}, {"key", key}
            };

            try
            {
                await _subscriber.SubscribeAsync($"{_channel}:{key}", (channel, message) =>
                {
                    _subscriber.UnsubscribeAsync(channel);
                    wrapper.ResultRedisValue = message;
                    wrapper.Task.Start();

                },_flags);
                wrapper.Action = () => { _subscriber.UnsubscribeAsync(_channel); };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                wrapper.Exception = e;
            }
            finally
            {
                if (wrapper.Exception == null)
                {
                    await _subscriber.PublishAsync(_channel, JsonConvert.SerializeObject(dictionary,Formatting.None),_flags);
                }
            }
        }
    }

   
    public class TaskWrapper
    {
        public Task Task { get; set; } = new Task(() => { });
        public Action Action { get; set; }
        public Exception Exception { get; set; }
        public RedisValue ResultRedisValue { get; set; }
    }
}
