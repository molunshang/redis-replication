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

        public static int ToInt(this byte[] bytes, int length = 4, bool isBigEndian = false)
        {
            int start, end, step;
            if (isBigEndian)
            {
                start = length - 1;
                end = 0;
                step = -1;
            }
            else
            {
                start = 0;
                end = length - 1;
                step = 1;
            }

            int result = bytes[start];
            for (; start <= end; start += step)
            {
                result = result << 8 | bytes[start];
            }

            return result;
        }

        public static long ToInt64(this byte[] bytes, int length = 8, bool isBigEndian = false)
        {
            int start, end, step;
            if (isBigEndian)
            {
                start = length - 1;
                end = 0;
                step = -1;
            }
            else
            {
                start = 0;
                end = length - 1;
                step = 1;
            }

            long result = bytes[start];
            for (; start <= end; start += step)
            {
                result = result << 8 | bytes[start];
            }

            return result;
        }

        public static uint ToUInt32(this byte[] bytes, int length = 4, bool isBigEndian = false)
        {
            int start, end, step;
            if (isBigEndian)
            {
                start = length - 1;
                end = 0;
                step = -1;
            }
            else
            {
                start = 0;
                end = length - 1;
                step = 1;
            }

            uint result = bytes[start];
            for (; start <= end; start += step)
            {
                result = result << 8 | bytes[start];
            }

            return result;
        }
    }
}