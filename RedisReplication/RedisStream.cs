using System;
using System.Buffers;
using System.IO;

namespace RedisReplication
{
    public class RedisStream : Stream
    {
//用单行回复，回复的第一个字节将是“+”
//错误消息，回复的第一个字节将是“-”
//整型数字，回复的第一个字节将是“:”
//批量回复，回复的第一个字节将是“$”
//多个批量回复，回复的第一个字节将是“*”
//        *<number of arguments> CR LF
//    $<number of bytes of argument 1> CR LF
//    <argument data> CR LF

        private const byte CR = (byte) '\r';
        private const byte LF = (byte) '\n';

        private byte[] _buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        private int _readIndex;
        private int _readLength;

        private readonly Stream _innerStream;

        private int FillBuffer()
        {
            var num = _readLength - _readIndex;
            if (num > 0 && _readIndex != 0)
            {
                Buffer.BlockCopy(_buffer, _readIndex, _buffer, 0, num);
            }

            if (_innerStream.CanRead)
            {
                _readLength = num + _innerStream.Read(_buffer, num, _buffer.Length - num);
                _readIndex = 0;
            }

            return _readLength;
        }

        public RedisStream(Stream innerStream)
        {
            this._innerStream = innerStream;
        }

        public override int ReadByte()
        {
            if (_readIndex >= _readLength)
            {
                FillBuffer();
            }

            return _buffer[_readIndex++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readSize = _readLength - _readIndex;
            if (readSize >= count)
            {
                Buffer.BlockCopy(this._buffer, _readIndex, buffer, offset, count);
                _readIndex += count;
                return count;
            }

            if (readSize > 0)
            {
                Buffer.BlockCopy(this._buffer, _readIndex, buffer, offset, readSize);
                _readIndex += readSize;
            }

            while (readSize < count)
            {
                var num = FillBuffer();
                if (num <= 0)
                {
                    return readSize;
                }

                num = Math.Min(count - readSize, num);
                Buffer.BlockCopy(this._buffer, _readIndex, buffer, offset, num);
                _readIndex += num;
                offset += num;
                readSize += num;
            }

            return readSize;
        }

        public byte[] ReadLineBytes()
        {
            FillBuffer();
            var currentIndex = _readIndex;
            MemoryStream result = null;
            while (true)
            {
                if (currentIndex >= _readLength)
                {
                    if (result == null)
                    {
                        result = new MemoryStream();
                    }

                    result.Write(_buffer, _readIndex, currentIndex - _readIndex);
                    FillBuffer();
                    currentIndex = _readIndex;
                }

                if (_buffer[currentIndex++] != CR || currentIndex >= _readLength || _buffer[currentIndex++] != LF)
                {
                    continue;
                }

                byte[] bytes;
                if (result != null)
                {
                    bytes = result.ToArray();
                }
                else
                {
                    var num = currentIndex - _readIndex - 2;
                    bytes = new byte[num];
                    Buffer.BlockCopy(_buffer, _readIndex, bytes, 0, num);
                }

                _readIndex = currentIndex;
                return bytes;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
    }
}