using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private readonly long _length;
        private static byte[][] _blocks;

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
                _length = sourceStream.Length;
                if (_length==0) throw new IOException("Cant read file");
                _countBlocks = _length >> 20;
            }
        }

        private void Read()
        {
            var pozition = 0;
            _blocks = new byte[_countBlocks + 1][];
            for (uint i = 0; i <= _countBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, pozition);
                _blocks[i] = new byte[i==_countBlocks ? _length % Constants.BlockLength : Constants.BlockLength];
                reader.ReadBlock(_blocks[i]);
                pozition += _blocks[i].Length;
            }
        }

        private void Zip()
        {
            var blockZipper = new BlockZipper(_blocks);
            blockZipper.ZipBlocks();
        }

        private void UnZip()
        {
            var blockZipper = new BlockZipper(_blocks);
            blockZipper.UnZipBlocks();
        }

        private void Write()
        {
            var pozition = 0;

            for (int i = 0; i <= _countBlocks; i++)
            {
                var blockWriter = new BlockWriter(_destFile, pozition);
                blockWriter.WriteBlock(_blocks[i]);
                pozition += _blocks[i].Length;
            }
        }

        public int Zipping()
        {
            int result = 0;
            try
            {
                var threadRead = new Thread(Read);
                threadRead.Start();
                threadRead.Join(Int32.MaxValue);
                var threadZip = new Thread(Zip);
                threadZip.Start();
                threadZip.Join(Int32.MaxValue);
                var threadWrite = new Thread(Write);
                threadWrite.Start();
                threadWrite.Join(Int32.MaxValue);
            }
            catch (Exception e)
            {
                result = 1;
            }

            return result;
        }

        public int Unzip()
        {
            int result=0;
            try
            {
                var threadRead = new Thread(Read);
                threadRead.Start();
                threadRead.Join(Int32.MaxValue);
                var threadUnZip = new Thread(UnZip);
                threadUnZip.Start();
                threadUnZip.Join(Int32.MaxValue);
                var threadWrite = new Thread(Write);
                threadWrite.Start();
                threadWrite.Join(Int32.MaxValue);
            }
            catch (Exception e)
            {
                result = 1;
            }
            return result;
        }
    }

}
