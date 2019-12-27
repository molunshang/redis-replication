using System;

namespace RedisReplication.Utils
{
    public static class Lzf
    {
        public static void Decode(byte[] source, byte[] target)
        {
            int inPos = 0, outPos = 0;
            do
            {
                var ctrl = source[inPos++];
                if (ctrl < 1 << 5)
                {
                    var len = ctrl + 1;
                    Array.Copy(source, inPos, target, outPos, len);
                    inPos += len;
                    outPos += len;
                }
                else
                {
                    var len = ctrl >> 5;
                    int offset;
                    if (len == 7) //批量重复型，解释字段3byte
                    {
                        len = source[inPos++] + 9;
                        offset = source[inPos++] + 1;
                    }
                    else //简短重复项 2byte
                    {
                        len += 2;
                        offset = (ctrl & 31 << 8 | source[inPos++]) + 1;
                    }

                    var refPos = outPos - offset;
                    Array.Copy(target, refPos, target, outPos, len);
                    outPos += len;
                }
            } while (inPos < source.Length);
        }
    }
}