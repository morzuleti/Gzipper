using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private readonly long _length;
        static AutoResetEvent waitHandler = new AutoResetEvent(true);

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

        private byte[][] Read()
        {
            var blocks = new byte[_countBlocks + 1][];

            var lastBlockLength = (int)(_length % Constants.BlockLength);
            if (lastBlockLength != 0)
            {
            blocks[_countBlocks] = new byte[lastBlockLength];
            var blockReader = new BlockReader(_sourceFile, (int)_countBlocks, lastBlockLength);
            blockReader.ReadBlock(blocks[_countBlocks]);
            }


            for (int i = 0; i < _countBlocks; i++)
            {
                blocks[i] = new byte[Constants.BlockLength];
                var reader = new BlockReader(_sourceFile, i, Constants.BlockLength);
                reader.ReadBlock(blocks[i]);
            }
            return blocks;
        }

        private byte[][] Zip(byte[][] blocks)
        {
            var blockZipper = new BlockZipper(blocks);
            blockZipper.ZipBlocks();
            return blocks;
        }

        private byte[][] UnZip(byte[][] blocks)
        {
            var blockZipper = new BlockZipper(blocks);
            blockZipper.UnZipBlocks();
            return blocks;
        }

        private int Write(byte[][] blocks)
        {
            var lastBlockLength = (int)(_length % Constants.BlockLength);
            blocks[_countBlocks] = new byte[lastBlockLength];
            var blockReader = new BlockWriter(_sourceFile, (int)_countBlocks, lastBlockLength);
            blockReader.WriteBlock(blocks[_countBlocks]);

            for (int i = 0; i < _countBlocks; i++)
            {
                blocks[i] = new byte[Constants.BlockLength];
                var reader = new BlockWriter(_sourceFile, i, Constants.BlockLength);
                reader.WriteBlock(blocks[i]);
            }

            return 0;
        }

        public int Zip()
        {
            int result;
            try
            {
                var blocks = Read();
                var zippedBlocks = Zip(blocks);
                result = Write(zippedBlocks);
            }
            catch (Exception e)
            {
                result = 1;
            }

            return result;
        }

        public int Unzip()
        {
            int result;
            try
            {
                var blocks = Read();
                var zippedBlocks = UnZip(blocks);
                result = Write(zippedBlocks);
            }
            catch (Exception e)
            {
                result = 1;
            }
            return result;
        }
    }

}
