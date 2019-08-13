using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipper
{
    public static class ZipperHelper
    {
        public  static IEnumerable<string> ReadBlockLength(string path, long numberOfSeparators, Encoding encoding, string separator)
        {
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(separator);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var foundSeparators = 0;
                var endPosition = fs.Length / sizeOfChar;

                for (var position = sizeOfChar + Constants.NumBytesAtCount; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);
                    if (encoding.GetString(buffer) == separator)
                    {
                        foundSeparators++;
                        if (foundSeparators == numberOfSeparators)
                        {
                            var returnBuffer = new byte[fs.Length - fs.Position - Constants.NumBytesAtCount];
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            var resultSting = encoding.GetString(returnBuffer);
                            return resultSting.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                var bytes = new byte[fs.Length - fs.Position];
                fs.Read(bytes, 0, bytes.Length);
                return encoding.GetString(bytes)
                    .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static long GetNumberOfBlocks(string path)
        {
            var buffer = new byte[Constants.NumBytesAtCount];
            using (var fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(-Constants.NumBytesAtCount, SeekOrigin.End);
                fs.Read(buffer, 0, buffer.Length);
            }

            var count = BitConverter.ToInt32(buffer, 0);

            return count;
        }
    }
}
