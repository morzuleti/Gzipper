using System;
using System.IO;
using System.Threading;

namespace GZipper
{
    class BlockReader
    {
        // создаем семафор
        private static Semaphore Sem = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        private readonly string _sourceFile;
        private readonly int _blockIndex;
        private readonly int _blockLength;

        public BlockReader(string sourceFile, int blockIndex, int blockLength)
        {
            _blockIndex = blockIndex;
            _blockLength = blockLength;
            _sourceFile = sourceFile;
        }

        public void ReadBlock(byte[] block)
        {
            var myThread = new Thread(Read);
            myThread.Start(block);
            myThread.Join(int.MaxValue);
        }

        private void Read(object blockObj)
        {
            Sem.WaitOne();
            var block = (byte[])blockObj;
            using (var sourceStream = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read,
                FileShare.Read, 4048, true))
            {
                sourceStream.Seek(_blockIndex * Constants.BlockLength, SeekOrigin.Begin);
               var rez = sourceStream.Read(block, 0, _blockLength);
            }
            Sem.Release();
        }
    }
}
