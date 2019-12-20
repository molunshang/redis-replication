using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RedisReplication.exceptions;

namespace RedisReplication
{
    public class RdbParser
    {
        private const byte MODULE_AUX = 247; //module相关辅助字段
        private const byte IDLE = 248; //lru空闲时间
        private const byte FREQ = 249; //lfu频率
        private const byte AUX = 250; //辅助字段类型
        private const byte RESIZEDB = 251; //存储键值数
        private const byte EXPIRETIME_MS = 252; //毫秒级过期时间
        private const byte EXPIRETIME = 253; //秒级过期时间
        private const byte SELECTDB = 254; //数据库序号
        private const byte EOF = 255; //文件结束标志

        private const byte RDB_TYPE_STRING = 0; //字符串
        private const byte RDB_TYPE_LIST_QUICKLIST = 14; //列表
        private const byte RDB_TYPE_SET = 2; //集合
        private const byte RDB_TYPE_SET_INTSET = 11; //集合
        private const byte RDB_TYPE_ZSET = 5; //有序集合
        private const byte RDB_TYPE_ZSET_ZIPLIST = 12; //有序集合
        private const byte RDB_TYPE_HASH = 4; //哈希

        private const byte RDB_TYPE_HASH_ZIPLIST = 255; //哈希
//        private const byte RDB_TYPE_LIST_ZIPLIST = 9; //

        private const string REDIS = "REDIS";

        private const byte LENGTH_MASK = byte.MaxValue >> 2;

        private Stream input;

        public RdbParser(Stream inputStream)
        {
            input = inputStream;
        }

        private string ReadString()
        {
            var strByteCounts = ReadLength();
            switch (strByteCounts)
            {
                case 1:
                case 2:
                case 4:
                    return input.ReadInt((int) strByteCounts, true).ToString();
                default:
                    throw new NotImplementedException();
            }
        }

        private uint ReadLength()
        {
            var flag = input.ReadByte();
            switch (flag >> 6)
            {
                case 0:
                    return (uint) flag & LENGTH_MASK;
                case 1:
                    return ((uint) flag & LENGTH_MASK) << 8 | (uint) input.ReadByte();
                case 2:
                    return input.ReadInt64(isBigEndian: true);
                case 3:
                    flag = flag * LENGTH_MASK;
                    switch (flag)
                    {
                        case 0:
                            return 1;
                        case 1:
                            return 2;
                        case 2:
                            return 4;
                        case 4:
                            var length = ReadLength();
                            ReadLength();
                            return length;
                    }

                    throw new RedisParserException();
                default:
                    throw new RedisParserException();
            }
        }

        public void StartParse()
        {
            var magic = input.ReadString(5);
            if (REDIS != magic)
            {
                throw new RedisParserException($"this is not redsi rdb file,the magic is {magic}");
            }

            var version = input.ReadString(4);
            Console.WriteLine($"rdb version is {version}");
            int dbNum = -1;
            var end = false;
            var auxDic = new Dictionary<string, string>();
            while (!end)
            {
                var opcode = input.ReadByte();
                switch (opcode)
                {
                    case MODULE_AUX:
                        auxDic.Add(ReadString(), ReadString());
                        break;
                    case FREQ:
                        break;
                    case AUX:
                        break;
                    case RESIZEDB:
                        break;
                    case EXPIRETIME_MS:
                        break;
                    case EXPIRETIME:
                        break;
                    case SELECTDB:
                        dbNum = (int) ReadLength();
                        break;
                    case EOF:
                        end = true;
                        break;
                }
            }
        }
    }
}