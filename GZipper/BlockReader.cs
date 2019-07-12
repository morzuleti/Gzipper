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
        private readonly long _position;

        public BlockReader(string sourceFile, long position)
        {
            _sourceFile = sourceFile;
            _position = position;
        }

        public void ReadBlock(byte[] block)
        {
            var myThread = new Thread(Read);
            myThread.Start(block);
            //myThread.Join(int.MaxValue);
        }

        private void Read(object blockObj)
        {
            Sem.WaitOne();
            var block = (byte[])blockObj;
            using (var sourceStream = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read,
                FileShare.Read, (int)Constants.BlockLength, true))
            {
                sourceStream.Seek(_position, SeekOrigin.Begin);
                sourceStream.Read(block, 0, block.Length);
            }
            Sem.Release();
        }
    }
}
