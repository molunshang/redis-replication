using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RedisReplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Encoding.UTF8.GetByteCount("123"));
            Console.WriteLine(Encoding.UTF8.GetString(new[] {(byte) 123}));
            var sb = new StringBuilder();
            while (true)
            {
                sb.Append(Guid.NewGuid());
                if (Encoding.UTF8.GetByteCount(sb.ToString()) > 255)
                {
                    Console.WriteLine(sb);
                    Console.WriteLine(Encoding.UTF8.GetByteCount(sb.ToString()));
                    break;
                }
            }

//            var memory = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

            var redisStream = new RedisStream(File.OpenRead("rdb.rdb"));
            while (true)
            {
                var bytes = redisStream.ReadLineBytes();
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
            }

//            var rdbStream = new FileStream("rdb.rdb", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
//            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
//            socket.Connect("10.255.73.155", 6385);
//            var commond = Encoding.UTF8.GetBytes("PSYNC 892003e1c11d497d087930706054593c06b65e41 94738044582\r\n");
//            var size = 0;
//            while (size < commond.Length)
//            {
//                size += socket.Send(commond);
//            }
//
//
//            var buffer = new byte[1024 * 4];
//            while (true)
//            {
//                size = socket.Receive(buffer);
//                rdbStream.Write(buffer, 0, size);
//                Console.WriteLine(size);
//                if (size < 0)
//                {
//                    return;
//                }
//            }
        }
    }
}