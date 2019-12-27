using System.IO;
using System.Text;

namespace RedisReplication.Utils
{
    public static class StreamUtils
    {
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
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return ByteUtils.ToInt(bytes, isBigEndian);
            });
        }

        public static short ReadInt16(this Stream stream, int size = 2, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return (short) (isBigEndian ? (short) bytes[0] << 8 | bytes[1] : (short) bytes[1] << 8 | bytes[0]);
            });
        }

        public static long ReadInt64(this Stream stream, int size = 8, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return ByteUtils.ToInt64(bytes, isBigEndian);
            });
        }

        public static uint ReadUInt32(this Stream stream, int size = 4, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadFullBytes(bytes);
                return ByteUtils.ToUInt32(bytes, isBigEndian);
            });
        }
    }
}