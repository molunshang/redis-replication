using System;

namespace RedisReplication.exceptions
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