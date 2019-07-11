using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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

        public MyGZipper(string sourceFile, string destFile)
        {
            _sourceFile = sourceFile;
            _destFile = destFile;
            using (var sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
            {
                _length = sourceStream.Length;
                _countBlocks = _length >> 20;
            }
        }

        private IEnumerable<byte[]> Read()
        {
            var blocks = new byte[_countBlocks + 1][];

            var lastBlockLength = (int)(_length % Constants.BlockLength);
            blocks[_countBlocks] = new byte[lastBlockLength];
            var blockReader = new BlockReader(_sourceFile, (int)_countBlocks, lastBlockLength);
            blockReader.ReadBlock(blocks[_countBlocks]);

            for (int i = 0; i < _countBlocks; i++)
            {
                blocks[i] = new byte[Constants.BlockLength];
                var reader = new BlockReader(_sourceFile, i, Constants.BlockLength);
                reader.ReadBlock(blocks[i]);
            }
            return blocks;
        }

        private int Write(IEnumerable<byte[]> sourceBytes, Work action)
        {
            using (var targetStream = File.OpenWrite(_destFile))
            {
                using (var compressionStream = new GZipStream(targetStream,
                    action == Work.Zip ? CompressionMode.Compress : CompressionMode.Decompress))
                {
                    foreach (var block in sourceBytes)
                    {
                        compressionStream.Write(block, 0, block.Length);
                    }
                }
            }

            return 0;
        }

        public int Zip()
        {
            int result;
            try
            {
                var blocks = Read();
                result = Write(blocks, Work.Zip);
            }
            catch
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
                result = Write(blocks, Work.Unzip);
            }
            catch
            {
                result = 1;
            }
            return result;
        }
    }

}
