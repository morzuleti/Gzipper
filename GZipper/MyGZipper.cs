﻿using System;
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
        private  long _countBlocks;
        private int _countBigBlocks;
        private static byte[][] _blocks;
        private readonly long _totalLength;

        public MyGZipper(string sourceFile, string destFile)
        {
            _sourceFile = sourceFile;
            _destFile = destFile;
            using (var sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
            {
                _totalLength = sourceStream.Length;
                if (_totalLength == 0) throw new IOException("Cant read file");
                _countBlocks = _totalLength >> 20;
            }
        }

        public int Zip()
        {
            int result = 0;
            try
            {
               _countBlocks ++;
               _countBigBlocks = (int) _countBlocks / Constants.BigBlockCount;
                for (var i = 0; i <= _countBigBlocks; i++)
                {
                    CompressZip(i);
                }
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
                _countBlocks = GetNumberOfBlocks(_sourceFile);
                _countBigBlocks = (int)_countBlocks / Constants.BigBlockCount;
                var blocksLength = ReadBlockLength(_sourceFile, _countBlocks, Encoding.UTF8, Constants.Separator).Select(int.Parse).ToArray();
                for (int i = 0; i <= _countBigBlocks; i++)
                {
                    DecompressZip(i, blocksLength);
                }
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }
            return result;
        }

        private void CompressZip(int numCurrentBlock)
        {
            ReadBlocks(numCurrentBlock, numCurrentBlock == _countBigBlocks? (int)_countBlocks%Constants.BigBlockCount:Constants.BigBlockCount);
            ArchiveProcessing(Work.Zip);
            WriteZip(numCurrentBlock);
        }

        private void ReadBlocks(int numCurrentBigBlock, int numberOfBlocks, int[] blocksLength = null)
        {
            long currentBlockToSkip = numCurrentBigBlock * Constants.BigBlockCount;
            long currentPosition = 0;
            for (int i = 0; i < currentBlockToSkip; i++)
            {
                currentPosition += blocksLength?[i]??Constants.BlockLength;
            }

            _blocks = new byte[numberOfBlocks][];
            for (long i = 0; i < numberOfBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, currentPosition);
                var lastBlockLength = (int) (numCurrentBigBlock == _countBigBlocks && i == numberOfBlocks - 1
                    ? _totalLength % Constants.BlockLength
                    : 0);
                _blocks[i] = reader.ReadBlock(blocksLength?[i+ currentBlockToSkip] ?? lastBlockLength);
                currentPosition += _blocks[i].Length;
            }
        }

        private static void ArchiveProcessing(Work workToDo)
        {
            var threads = new Thread[_blocks.Length];
            for (var i = 0; i < _blocks.Length; i++)
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

        private void WriteZip(int numCurrentBigBlock)
        {
            WriteFile();
            if (numCurrentBigBlock==_countBigBlocks)
            { 
                BlockWriter.FinalizeFile();
            }
        }

        private void WriteFile()
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                var blockWriter = new BlockWriter(_destFile);
                blockWriter.WriteBlock(_blocks[i]);
            }
        }

        private void DecompressZip(int numCurrentBigBlock, int[] blocksLength)
        {

            var numberOfBlocks = numCurrentBigBlock == _countBigBlocks
                ? _countBlocks % Constants.BigBlockCount
                : Constants.BigBlockCount;
             ReadBlocks(numCurrentBigBlock, (int)numberOfBlocks, blocksLength);
             ArchiveProcessing(Work.Unzip);
             WriteFile();
        }

        private long GetNumberOfBlocks(string path)
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
