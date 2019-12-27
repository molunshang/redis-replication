using System;
using System.Buffers;
using System.Linq;

namespace RedisReplication.Utils
{
    public static class ByteUtils
    {
        private static readonly ArrayPool<byte> RedisMemoryPool = ArrayPool<byte>.Create();

        public static T RequireBytes<T>(int size, Func<byte[], T> func)
        {
            var bytes = RedisMemoryPool.Rent(size);
            try
            {
                return func(bytes);
            }
            finally
            {
                RedisMemoryPool.Return(bytes);
            }
        }

        public static int ToInt(this byte[] bytes, bool isBigEndian = false)
        {
            return (isBigEndian ? bytes.Reverse() : bytes).Aggregate((int) bytes[isBigEndian ? bytes.Length - 1 : 0],
                (current, b) => current << 8 | b);
        }

        public static long ToInt64(this byte[] bytes, bool isBigEndian = false)
        {
            return (isBigEndian ? bytes.Reverse() : bytes).Aggregate((long) bytes[isBigEndian ? bytes.Length - 1 : 0],
                (current, b) => current << 8 | b);
        }

        public static uint ToUInt32(this byte[] bytes, bool isBigEndian = false)
        {
            return (isBigEndian ? bytes.Reverse() : bytes).Aggregate((uint) bytes[isBigEndian ? bytes.Length - 1 : 0],
                (current, b) => current << 8 | b);
        }
    }
}