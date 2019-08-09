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
                var length = sourceStream.Length;
                if (length == 0) throw new IOException("Cant read file");
                _countBlocks = length >> 20;
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

        private void CompressZip()
        {
            ReadBlocks((int)_countBlocks + 1);
            ArchiveProcessing((int)_countBlocks + 1, Work.Zip);
            WriteZip();
        }

        private void ReadBlocks(int numberOfBlocks, int[] blocksLength = null)
        {
            var currentPosition = 0;
            _blocks = new byte[numberOfBlocks][];
            for (int i = 0; i < numberOfBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, currentPosition);
                _blocks[i] = reader.ReadBlock(blocksLength?[i] ?? 0);
                currentPosition += _blocks[i].Length;
            }
        }

        private static void ArchiveProcessing(int numberOfBlocks, Work workToDo)
        {
            var threads = new Thread[numberOfBlocks];
            for (var i = 0; i < numberOfBlocks; i++)
            {
                var blockZipper = new BlockZipper();
                var data = new Data(_blocks[i], workToDo, i);
                blockZipper.ZippedEvent += (sender, args) => { _blocks[args.IndexOfArray] = args.ZippedBytes; };
                threads[i] = blockZipper.TreatBlock(data);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
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
            var blocksLength = ReadBlockLength(_sourceFile, numberOfBlocks, Encoding.UTF8, Constants.Separator).Select(int.Parse).ToArray();
            ReadBlocks(numberOfBlocks, blocksLength);
            ArchiveProcessing(numberOfBlocks, Work.Unzip);
            _blocks[numberOfBlocks - 1] = ClearEnd(_blocks[numberOfBlocks - 1]);
            WriteFile();
        }

        private byte[] ClearEnd(byte[] initBytes)
        {
            for (int i = 5; i < initBytes.Length; i++)
            {
                if (initBytes[i - 5] == 0 && initBytes[i - 4] == 0 && initBytes[i - 3] == 0 && initBytes[i - 2] == 0 &&
                    initBytes[i - 1] == 0 && initBytes[i] == 0)
                {
                    var eof = i - 5;
                    byte[] result = new byte[initBytes.Length - eof];
                    Array.Copy(initBytes, result, initBytes.Length - eof);
                    return result;
                }
            }
            return initBytes;
        }

        private int GetNumberOfBlocks(string path)
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

        private static IEnumerable<string> ReadBlockLength(string path, Int64 numberOfSeparators, Encoding encoding, string separator)
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
    }
}
