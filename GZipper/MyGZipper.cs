using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GZipper
{
    public interface IZipper
    {
        int Zip();
        int Unzip();
    }
    public delegate void ZippingFinished(byte[][] zippedBytes, int numCurrentBlock);
    public class MyGZipper : IZipper
    {
        private readonly string _sourceFile;
        private readonly string _destFile;
        private  long _countBlocks;
        private static int _countBigBlocks;
        private static int _cacheSize = Constants.ThreadCount;
        private static IIoProcessor _ioProcessor;
        private static ZippingFinished _zippingFinished;
        private readonly long _totalLength;
        private static object _lock = new object();
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
                _countBlocks++;
                _countBigBlocks = (int) _countBlocks / Constants.BigBlockCount;
                _ioProcessor = new IoProcessor(_sourceFile, _destFile, _countBigBlocks, _totalLength);
                _zippingFinished += WriteZip;

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
                _countBlocks = ZipperHelper.GetNumberOfBlocks(_sourceFile);
                _countBigBlocks = (int)_countBlocks / Constants.BigBlockCount;
                _ioProcessor = new IoProcessor(_sourceFile, _destFile, _countBigBlocks, _totalLength);
                var blocksLength = ZipperHelper.ReadBlockLength(_sourceFile, _countBlocks, Encoding.UTF8, Constants.Separator).Select(int.Parse).ToArray();
                _zippingFinished += WriteFile;
                _cacheSize /= 2;
                for (var i = 0; i <= _countBigBlocks; i++)
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
            var block = _ioProcessor.ReadBlocks(numCurrentBlock, numCurrentBlock == _countBigBlocks ? (int)_countBlocks % Constants.BigBlockCount : Constants.BigBlockCount);
            ArchiveProcessing(block, numCurrentBlock, Work.Zip);
        }

        private static void ArchiveProcessing(byte[][]bigBlock,int numCurrentBlock, Work workToDo)
        {
            for (var i = 0; i < bigBlock.Length; i++)
            {
                IBlockZipper blockZipper = new BlockZipper();
                var data = new Data(bigBlock[i], workToDo, i);
                blockZipper.ZippedEvent += (sender, args) => 
                {
                    bigBlock[args.IndexOfArray] = args.ZippedBytes;
                    if (args.IndexOfArray == bigBlock.Length-1)
                    {
                        _zippingFinished?.Invoke(bigBlock, numCurrentBlock);
                    }
                };
                blockZipper.TreatBlock(data);
            }
        }


        private void WriteZip(byte[][] bigBlock, int numCurrentBigBlock)
        {
            WriteFile(bigBlock, numCurrentBigBlock);
            if (numCurrentBigBlock == _countBigBlocks)
            {
                BlockWriter.FinalizeFile();
            }
        }

        private  Dictionary <int,byte[][]> cache = new Dictionary<int,byte[][]>(_countBigBlocks/2);
        private void WriteFile(byte[][] bigBlock, int numCurrentBigBlock)
        {
            Monitor.Enter(_lock);
            cache.Add(numCurrentBigBlock, bigBlock);
            if (cache.Count == _cacheSize || numCurrentBigBlock == _countBigBlocks)
            {
                foreach (var key in cache.Keys.OrderBy(val=>val))
                {
                    _ioProcessor.WriteFile(cache[key]);
                }
                cache.Clear();
            }
            Monitor.Exit(_lock);
        }

        private void DecompressZip(int numCurrentBigBlock, int[] blocksLength)
        {

            var numberOfBlocks = numCurrentBigBlock == _countBigBlocks
                ? (_countBlocks % Constants.BigBlockCount)==0?1: _countBlocks % Constants.BigBlockCount
                : Constants.BigBlockCount;
            var block = _ioProcessor.ReadBlocks(numCurrentBigBlock, (int)numberOfBlocks, blocksLength);
             ArchiveProcessing(block, numCurrentBigBlock, Work.Unzip);
        }
    }
}
