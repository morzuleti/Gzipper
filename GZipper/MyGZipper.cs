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

        private byte[][] UnZip(byte[][] blocks)
        {
            var blockZipper = new BlockZipper(blocks);
            blockZipper.UnZipBlocks();
            return blocks;
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
                var myThread1 = new Thread(Read);
                myThread1.Start();
                myThread1.Join(int.MaxValue);
                var myThread2 = new Thread(Zip);
                myThread2.Start();
                myThread2.Join(int.MaxValue);
                var myThread = new Thread(Write);
                myThread.Start();
                myThread.Join(int.MaxValue);
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
                //var blocks = Read();
                //var zippedBlocks = UnZip(blocks);
               // result = Write(zippedBlocks);
            }
            catch (Exception e)
            {
                result = 1;
            }
            return result;
        }
    }

}
