using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipper
{
    public interface IZipper
    {
        int Zip();
        int Unzip();
    }

    public class MyGZipper : IZipper
    {
        private readonly string _sourceFile;
        private readonly string _destFile;
        private readonly long _countBlocks;
        private static byte[][] _blocks;

        public MyGZipper(string sourceFile, string destFile)
        {
            _sourceFile = sourceFile;
            _destFile = destFile;
            using (var sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
            {
                var  length = sourceStream.Length;
                if (length == 0) throw new IOException("Cant read file");
                _countBlocks = length >> 20;
            }
        }

        private void CompressZip()
        {
            var currentPosition = 0;
            _blocks = new byte[_countBlocks + 1][];
            for (var i = 0; i <= _countBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, currentPosition);
                _blocks[i] = reader.ReadBlock();
                currentPosition += _blocks[i].Length;
            }

            var threads = new Thread[_countBlocks + 1];
            for (var i = 0; i <= _countBlocks; i++)
            {
                var blockZipper = new BlockZipper();
                var data = new Data(_blocks[i], Work.Zip, i);
                blockZipper.ZippedEvent += (sender, args) => { _blocks[args.IndexOfArray] = args.ZippedBytes; };
                threads[i] = blockZipper.ZipBlock(data);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            WriteZip();
        }

        private void WriteZip()
        {
            WriteFile();
            BlockWriter.FinalizeFile();
        }

        private void WriteFile()
        {
            var pozition = 0;
            for (int i = 0; i < _blocks.Length; i++)
            {
                var blockWriter = new BlockWriter(_destFile, pozition);
                blockWriter.WriteBlock(_blocks[i]);
                pozition += _blocks[i].Length;
            }
        }

        private void DecompressZip()
        {
            var numberOfBlocks = GetNumberOfBlocks(_sourceFile);
            var blocksLength = ReadBlockLength(_sourceFile, numberOfBlocks, Encoding.UTF8, Constants.Separator).ToArray();
            var currentPosition = 0;
            _blocks = new byte[numberOfBlocks][];
            for (int i = 0; i < numberOfBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, currentPosition);
                if (int.TryParse(blocksLength[i], out var currBlockLength))
                {
                    _blocks[i] = reader.ReadBlock(currBlockLength);
                }

                currentPosition += _blocks[i].Length;
            }


            var threads = new Thread[numberOfBlocks];
            for (var i = 0; i < numberOfBlocks; i++)
            {
                var blockZipper = new BlockZipper();
                var data = new Data(_blocks[i], Work.Unzip, i);
                blockZipper.ZippedEvent += (sender, args) => { _blocks[args.IndexOfArray] = args.ZippedBytes; };
                threads[i] = blockZipper.ZipBlock(data);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            WriteFile();
        }


        private int GetNumberOfBlocks (string path)
        {
            var buffer = new byte[Constants.NumBytesAtCount];
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(-Constants.NumBytesAtCount, SeekOrigin.End);
                fs.Read(buffer, 0, buffer.Length);
            }

            var count = BitConverter.ToInt32(buffer, 0);
            return count;
        }

        public static IEnumerable<string> ReadBlockLength(string path, Int64 numberOfSeparators, Encoding encoding, string separator)
        {
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(separator);

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Int64 foundSeparators = 0;
                Int64 endPosition = fs.Length / sizeOfChar;

                for (Int64 position = sizeOfChar + Constants.NumBytesAtCount; position < endPosition; position += sizeOfChar)
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
                            return resultSting.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                var bytes = new byte[fs.Length - fs.Position];
                fs.Read(bytes, 0, bytes.Length);
                return encoding.GetString(bytes)
                    .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public int Zip()
        {
            int result = 0;
            try
            {
               CompressZip();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }

            return result;
        }

        public int Unzip()
        {
            int result = 0;
            try
            {
                DecompressZip();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }
            return result;
        }
    }

}
