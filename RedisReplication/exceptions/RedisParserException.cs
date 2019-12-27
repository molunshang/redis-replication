using System;

namespace RedisReplication.Exceptions
{
    public class RedisParserException : Exception
    {
        public RedisParserException()
        {
        }

        public RedisParserException(string message) : base(message)
        {
        }
    }
}