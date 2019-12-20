using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace RedisReplication
{
    public static class StreamUtils
    {
        private static readonly ArrayPool<byte> redisMemoryPool = ArrayPool<byte>.Create();

        private static T RequireBytes<T>(int size, Func<byte[], T> func)
        {
            var bytes = redisMemoryPool.Rent(size);
            try
            {
                return func(bytes);
            }
            finally
            {
                redisMemoryPool.Return(bytes);
            }
        }

        public static void ReadFullBytes(this Stream stream, byte[] bytes)
        {
            var readNum = 0;
            while (readNum < bytes.Length)
            {
                readNum += stream.Read(bytes, readNum, bytes.Length - readNum);
            }
        }


        public static string ReadString(this Stream stream, int size)
        {
            var bytes = new byte[size];
            ReadFullBytes(stream, bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static int ReadInt(this Stream stream, int size = 4, bool isBigEndian = false)
        {
            return RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return bytes.ToInt(isBigEndian);
            });
        }

        public static uint ReadInt64(this Stream stream, int size = 4, bool isBigEndian = false)
        {
            return RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return bytes.ToInt64(isBigEndian);
            });
        }
    }
}