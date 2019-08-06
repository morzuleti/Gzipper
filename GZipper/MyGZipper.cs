using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace GZipper
{
    public interface IZipper
    {
        int Zipping();
        int Unzip();
    }

    public class MyGZipper : IZipper
    {
        private readonly string _sourceFile;
        private readonly string _destFile;
        private readonly long _countBlocks;
        private static byte[][] _blocks;
        private static readonly Semaphore SemRead = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        private const int NumBytesAtCount = 4;
        private static readonly byte[] _headerBytes = new byte[10]
        {
            31,
            139,
            8,
            0,
            0,
            0,
            0,
            0,
            4,
            0
        };

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

        private void Read()
        {
            var currentPosition = 0;
            _blocks = new byte[_countBlocks + 1][];
            for (uint i = 0; i <= _countBlocks; i++)
            {
                SemRead.WaitOne();
                var reader = new BlockReader(_sourceFile, currentPosition);
                //_blocks[i] = new byte[i == _countBlocks ? _length % Constants.BlockLength : Constants.BlockLength];
                _blocks[i] = reader.ReadBlock();
                currentPosition += _blocks[i].Length;
                SemRead.Release();
                var blockZipper = new BlockZipper();
                _blocks[i] = blockZipper.ZipBlock(_blocks[i]).ToArray();
              
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

        private void ReadZip()
        {
            var numberOfBlocks = GetNumberOfBlocks(_sourceFile);
            var blocksLength = ReadBlockLength(_sourceFile, numberOfBlocks, Encoding.UTF8, "Ё!~").ToArray();
            var currentPosition = 0;
            _blocks = new byte[numberOfBlocks][];
            for (uint i = 0; i < numberOfBlocks; i++)
            {
                SemRead.WaitOne();
                var reader = new BlockReader(_sourceFile, currentPosition);
                //_blocks[i] = new byte[i == _countBlocks ? _length % Constants.BlockLength : Constants.BlockLength];
                if (int.TryParse(blocksLength[i], out var currBlockLength))
                {
                    _blocks[i] = reader.ReadBlock(currBlockLength);
                }
                currentPosition += _blocks[i].Length;
                SemRead.Release();
                var blockZipper = new BlockZipper();
                _blocks[i] = blockZipper.UnZipBlock(_blocks[i]).ToArray();
            }

            WriteFile();
        }


        private int GetNumberOfBlocks (string path)
        {
            var buffer = new byte[NumBytesAtCount];
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(-NumBytesAtCount, SeekOrigin.End);
                fs.Read(buffer, 0, buffer.Length);
            }

            var count = BitConverter.ToInt32(buffer, 0);
            return count;
        }

        public static IEnumerable<string> ReadBlockLength(string path, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
        {

            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(tokenSeparator);


            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Int64 tokenCount = 0;
                Int64 endPosition = fs.Length / sizeOfChar;

                for (Int64 position = sizeOfChar + NumBytesAtCount; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);
                    if (encoding.GetString(buffer) == tokenSeparator)
                    {
                        tokenCount++;
                        if (tokenCount == numberOfTokens)
                        {
                            var returnBuffer = new byte[fs.Length - fs.Position- NumBytesAtCount];
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            var resultSting = encoding.GetString(returnBuffer);
                            return resultSting.Split(new[] {tokenSeparator}, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                var bytes = new byte[fs.Length - fs.Position];
                fs.Read(bytes, 0, bytes.Length);
                return encoding.GetString(bytes)
                    .Split(new[] { tokenSeparator }, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        //private void UnZip()
        //{
        //    var blockZipper = new BlockZipper(_blocks);
        //    blockZipper.UnZipBlock();
        //}


        //private void WriteBlocks()
        //{
        //    var pozition = 0;

        //    var blockWriter = new BlockWriter(_destFile, pozition, _blocks.Length);
        //    for (int i = 0; i < _blocks.Length; i++)
        //    {
        //        blockWriter.WriteBlock(_blocks[i]);
        //        pozition += _blocks[i].Length;
        //    }

        //}

        public int Zipping()
        {
            int result = 0;
            try
            {
                var threadRead = new Thread(Read);
                threadRead.Start();
                threadRead.Join(Int32.MaxValue);
            }
            catch (Exception e)
            {
                result = 1;
            }

            return result;
        }

        public int Unzip()
        {
            int result = 0;
            try
            {
                var threadRead = new Thread(ReadZip);
                threadRead.Start();
                threadRead.Join(Int32.MaxValue);
                //var threadUnZip = new Thread(UnZip);
                //threadUnZip.Start();
                //threadUnZip.Join(Int32.MaxValue);
                //var threadWrite = new Thread(WriteBlocks(););
                //threadWrite.Start();
                //threadWrite.Join(Int32.MaxValue);
            }
            catch (Exception e)
            {
                result = 1;
            }
            return result;
        }
    }

}
