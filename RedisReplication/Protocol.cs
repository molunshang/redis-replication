using System;
using System.IO;

namespace RedisReplication
{
    public static class Protocol
    {
//用单行回复，回复的第一个字节将是“+”
//错误消息，回复的第一个字节将是“-”
//整型数字，回复的第一个字节将是“:”
//批量回复，回复的第一个字节将是“$”
//多个批量回复，回复的第一个字节将是“*”
//        *<number of arguments> CR LF
//    $<number of bytes of argument 1> CR LF
//    <argument data> CR LF
        private const byte PLUS = (byte) '+';
        private const byte MINUS = (byte) '-';
        private const byte COLON = (byte) ':';
        private const byte DOLLAR = (byte) '$';
        private const byte ASTERISK = (byte) '*';
        private const byte CR = (byte) '\r';
        private const byte LF = (byte) '\n';

        public static byte[] Read(this Stream stream)
        {
            var flag = stream.ReadByte();
            switch (flag)
            {
                case PLUS:
                    break;
                case MINUS:
                    break;
                case COLON:
                    break;
                case DOLLAR:
                    break;
                case ASTERISK:
                    break;
                default:
                    throw new Exception($"unknown flag {flag}");
            }
            throw new NotImplementedException();
        }
    }
}