using System;
using System.IO;

namespace OnChrome.Core
{
    public static class StreamExtensions
    {
        public static byte[] ReadExactly(this Stream stream, int size)
        {
            var result = new byte[size];
            var readInto = result.AsSpan();
            while (!readInto.IsEmpty)
            {
                var readChars = stream.Read(readInto);
                if (readChars == -1)
                {
                    throw new Exception("Reached the end of the stream unexpectedly");
                }

                readInto = readInto.Slice(readChars);
            }

            return result;
        }
    }
}
