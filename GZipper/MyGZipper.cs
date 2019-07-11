using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public MyGZipper(string sourceFile, string destFile)
        {
            _sourceFile = sourceFile;
            _destFile = destFile;
        }

        private byte[][] Read()
        {
            long countBlocks;
            long length;
            using (FileStream sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
            {
                length = sourceStream.Length;
                countBlocks = length >> 20;
            }
            byte[][] blocks = new byte[countBlocks + 1][];

            int lastBlockLength = (int)(length % Constants.BlockLength);
            blocks[countBlocks] = new byte[lastBlockLength];
            var blockReader = new BlockReader(_sourceFile, (int)countBlocks, lastBlockLength);
            blockReader.ReadBlock(blocks[countBlocks]);

            for (int i = 0; i < countBlocks; i++)
            {
                blocks[i] = new byte[Constants.BlockLength];
                var reader = new BlockReader(_sourceFile, i, Constants.BlockLength);
                reader.ReadBlock(blocks[i]);
            }
            return blocks;
        }

        private int Write(byte[][] sourceBytes, Work action)
        {
            using (FileStream targetStream = File.OpenWrite(_destFile))
            {
                using (GZipStream compressionStream = new GZipStream(targetStream,
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
