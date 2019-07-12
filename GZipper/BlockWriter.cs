using System.IO;
using System.Threading;

namespace GZipper
{
    class BlockWriter
    {
        // создаем семафор
        private static readonly Semaphore Sem = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        private readonly string _destFile;
        private readonly int _blockPoz;

        public BlockWriter(string destFile, int blockPoz)
        {
            _blockPoz = blockPoz;
            _destFile = destFile;
        }

        public void WriteBlock(byte[] block)
        {
            var myThread = new Thread(Write);
            myThread.Start(block);
            myThread.Join(int.MaxValue);
        }

        private void Write(object blockObj)
        {
            Sem.WaitOne();
            var block = (byte[])blockObj;
            using (var sourceStream = new FileStream(_destFile, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.Write, 4048, true))
            {
                sourceStream.Seek(_blockPoz, SeekOrigin.Begin);
                sourceStream.Write(block, 0, block.Length);
            }
            Sem.Release();
        }
    }
}