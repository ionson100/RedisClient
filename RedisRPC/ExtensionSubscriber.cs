using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRPC
{
    public static class ExtensionSubscriber
    {
        /// <summary>
        /// Процедура исполителя Call запроса
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="channel">Канал запроса</param>
        /// <param name="handler">Исполение запроса</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static async Task  PerformerRpcAsync(this ISubscriber subscriber,
            RedisChannel channel, Func<RedisChannel, RedisValue, string> handler, CommandFlags flags = CommandFlags.None)
        {
            InnerPerformerWorker worker=new InnerPerformerWorker();
            await worker.Perform(subscriber, channel,handler);
        }
        /// <summary>
        /// Процедура запроса на удаленное исполнение
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="channel">Канал запроса</param>
        /// <param name="param">Строковой параметр для процедуры исполнения</param>
        /// <param name="timeOut">Время ожидания исполения процедуры</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static async Task<string> CallerRcpExtAsync(this ISubscriber subscriber, RedisChannel channel,string param,int timeOut=1000, 
            CommandFlags flags = CommandFlags.None)
        {
            InnerCallWorker innerCallWorker=new InnerCallWorker(subscriber,channel,flags);
            try
            {
                var dd = await innerCallWorker.Call(param, timeOut);
                return dd;
            }
           
            catch (Exception e)
            {
                Console.WriteLine(e);
                if ((e as InnerExceptionRpc) != null)
                {
                    throw new Exception(e.Message);
                }
                throw new Exception(e.Message,e);
            }
        }

    }
}
