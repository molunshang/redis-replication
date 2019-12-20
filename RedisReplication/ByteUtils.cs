using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;

namespace RedisReplication
{
    public static class ByteUtils
    {
        public static int ToInt(this byte[] bytes, bool isBigEndian = false)
        {
            return (isBigEndian ? bytes.Reverse() : bytes).Aggregate((int) bytes[isBigEndian ? bytes.Length - 1 : 0],
                (current, b) => current << 8 | b);
        }

        public static uint ToInt64(this byte[] bytes, bool isBigEndian = false)
        {
            return (isBigEndian ? bytes.Reverse() : bytes).Aggregate((uint) bytes[isBigEndian ? bytes.Length - 1 : 0],
                (current, b) => current << 8 | b);
        }
    }
}