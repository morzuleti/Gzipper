using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipper
{
    class BlockWriter
    {
        // создаем семафор
        static Semaphore sem = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        private readonly string _destFile;
        private readonly int _blockIndex;
        private readonly int _blockLength;

        public BlockWriter(string destFile, int blockIndex, int blockLength)
        {
            _blockIndex = blockIndex;
            _blockLength = blockLength;
            _destFile = destFile;
        }

        public void WriteBlock(byte[] block)
        {
            var myThread = new Thread(Write);
            myThread.Start(block);
        }

        private void Write(object blockObj)
        {
            sem.WaitOne();
            var block = (byte[])blockObj;
            using (var sourceStream = new FileStream(_destFile, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.Write, 4048, true))
            {
                sourceStream.Seek(_blockIndex * Constants.BlockLength, SeekOrigin.Begin);
                sourceStream.Write(block, 0, _blockLength);
            }
            sem.Release();
        }
    }
}
