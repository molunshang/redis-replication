using System.IO;
using System.Text;

namespace RedisReplication.Utils
{
    public static class StreamUtils
    {
        public static void ReadBytes(this Stream stream, byte[] bytes, int num = -1)
        {
            if (num <= 0)
            {
                num = bytes.Length;
            }
            var readNum = 0;
            while (readNum < num)
            {
                readNum += stream.Read(bytes, readNum, num - readNum);
            }
        }

        public static string ReadString(this Stream stream, int size)
        {
            var bytes = new byte[size];
            ReadBytes(stream, bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static int ReadInt(this Stream stream, int size = 4, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadBytes(bytes,size);
                return bytes.ToInt(isBigEndian:isBigEndian);
            });
        }

        public static short ReadInt16(this Stream stream, int size = 2, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadBytes(bytes);
                return (short) (isBigEndian ? (short) bytes[0] << 8 | bytes[1] : (short) bytes[1] << 8 | bytes[0]);
            });
        }

        public static long ReadInt64(this Stream stream, int size = 8, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadBytes(bytes);
                return bytes.ToInt64(isBigEndian:isBigEndian);
            });
        }

        public static uint ReadUInt32(this Stream stream, int size = 4, bool isBigEndian = false)
        {
            return ByteUtils.RequireBytes(size, bytes =>
            {
                stream.ReadBytes(bytes);
                return bytes.ToUInt32(isBigEndian:isBigEndian);
            });
        }
    }
}