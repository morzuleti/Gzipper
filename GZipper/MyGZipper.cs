using System;
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
            using (var sourceStream = new FileStream(_sourceFile, FileMode.Open))
            {
                _totalLength = sourceStream.Length;
                if (_totalLength == 0) throw new IOException("Cant read file");
                _countBlocks = _totalLength >> 20;
            }

            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
        }

        public int Zip()
        {
            int result = 0;
            try
            {
               _countBlocks ++;
               _countBigBlocks = (int) _countBlocks / Constants.BigBlockCount;
               var ioProcessor = new IoProcessor(_sourceFile, _destFile, _countBigBlocks, _totalLength);
                for (var i = 0; i <= _countBigBlocks; i++)
                {
                    CompressZip(i, ioProcessor);
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
                _countBlocks = ZipperHelper.GetNumberOfBlocks(_sourceFile);
                _countBigBlocks = (int)_countBlocks / Constants.BigBlockCount;
                var ioProcessor = new IoProcessor(_sourceFile, _destFile, _countBigBlocks, _totalLength);
                var blocksLength = ZipperHelper.ReadBlockLength(_sourceFile, _countBlocks, Encoding.UTF8, Constants.Separator).Select(int.Parse).ToArray();
                for (var i = 0; i <= _countBigBlocks; i++)
                {
                    DecompressZip(i, blocksLength, ioProcessor);
                }
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }
            return result;
        }

        private void CompressZip(int numCurrentBlock, IIoProcessor ioProcessor)
        {
            _blocks = ioProcessor.ReadBlocks(numCurrentBlock, numCurrentBlock == _countBigBlocks? (int)_countBlocks%Constants.BigBlockCount:Constants.BigBlockCount);
            ArchiveProcessing(Work.Zip);
            WriteZip(numCurrentBlock, ioProcessor);
        }

        private static void ArchiveProcessing(Work workToDo)
        {
            var threads = new Thread[_blocks.Length];
            for (var i = 0; i < _blocks.Length; i++)
            {
                IBlockZipper blockZipper = new BlockZipper();
                var data = new Data(_blocks[i], workToDo, i);
                blockZipper.ZippedEvent += (sender, args) => { _blocks[args.IndexOfArray] = args.ZippedBytes; };
                threads[i] = blockZipper.TreatBlock(data);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private void WriteZip(int numCurrentBigBlock, IIoProcessor ioProcessor)
        {
            ioProcessor.WriteFile(_blocks);
            if (numCurrentBigBlock == _countBigBlocks)
            { 
                BlockWriter.FinalizeFile();
            }
        }

        private void DecompressZip(int numCurrentBigBlock, int[] blocksLength , IIoProcessor ioProcessor)
        {

            var numberOfBlocks = numCurrentBigBlock == _countBigBlocks
                ? _countBlocks % Constants.BigBlockCount
                : Constants.BigBlockCount;
             _blocks = ioProcessor.ReadBlocks(numCurrentBigBlock, (int)numberOfBlocks, blocksLength);
             ArchiveProcessing(Work.Unzip);
             ioProcessor.WriteFile(_blocks);
        }
    }
}
