using System;

namespace RedisRPC
{
   public class InnerExceptionRpc:Exception
    {
        public InnerExceptionRpc(string message):base(message){}
    }
}
