using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RedisReplication.Exceptions;
using RedisReplication.Utils;

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
        private const byte LENGTH_00 = 1 << 7;
        private const byte LENGTH_01 = LENGTH_00 | 1;

        private const byte STRING_00 = 3 << 6; //11000000
        private const byte STRING_01 = STRING_00 | 1; //11000001
        private const byte STRING_10 = STRING_00 | 2; //11000010
        private const byte STRING_11 = STRING_00 | 3; //11000011

        private Stream input;

        public RdbParser(Stream inputStream)
        {
            input = inputStream;
        }

        private string ReadString()
        {
            var length = ReadLength(out var encoded);
            if (encoded)
            {
                switch (length)
                {
                    case STRING_00:
                        return ((sbyte) input.ReadByte()).ToString();
                    case STRING_01:
                        return input.ReadInt16(isBigEndian: true).ToString();
                    case STRING_10:
                        return input.ReadInt(isBigEndian: true).ToString();
                    case STRING_11:
                        int compressLength = (int) ReadLength(out encoded),
                            originalLength = (int) ReadLength(out encoded);
                        return ByteUtils.RequireBytes(compressLength, sources =>
                        {
                            input.ReadBytes(sources, compressLength);
                            return ByteUtils.RequireBytes(originalLength, target =>
                            {
                                input.ReadBytes(target, originalLength);
                                Lzf.Decode(sources, target);
                                return Encoding.UTF8.GetString(target, 0, originalLength);
                            });
                        });
                }
            }

            var len = (int) length;
            return ByteUtils.RequireBytes(len, bytes =>
            {
                input.ReadBytes(bytes, len);
                return Encoding.UTF8.GetString(bytes, 0, len);
            });
        }

        private long ReadLength(out bool isEncoded)
        {
            isEncoded = false;
            var flag = input.ReadByte();
            switch (flag >> 6)
            {
                case 0:
                    return (uint) flag & LENGTH_MASK;
                case 1:
                    return ((uint) flag & LENGTH_MASK) << 8 | (uint) input.ReadByte();
                case 2:
                    switch (flag)
                    {
                        case LENGTH_00:
                            return input.ReadUInt32(isBigEndian: true);
                        case LENGTH_01:
                            return input.ReadInt64(isBigEndian: true);
                    }

                    break;
                case 3:
                    isEncoded = true;
                    return flag;
            }

            throw new RedisParserException();
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
                    case IDLE:
                        break;
                    case FREQ:
                        break;
                    case AUX:
                    case EXPIRETIME_MS:
                        auxDic.Add("expiretime-ms", input.ReadInt64().ToString());
                        break;
                    case EXPIRETIME:
                        auxDic.Add("expiretime-s", input.ReadInt().ToString());
                        break;
                    case SELECTDB:
                        dbNum = (int) ReadLength(out _);
                        auxDic.Add("db", dbNum.ToString());
                        break;
                    case RESIZEDB:
                        auxDic.Add("db_size", ReadLength(out _).ToString());
                        auxDic.Add("expires_size", ReadLength(out _).ToString());
                        break;
                    case EOF:
                        end = true;
                        break;
                }
            }
        }
    }
}