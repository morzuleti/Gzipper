using System.IO;
using System.Threading;

namespace GZipper
{
    class BlockReader
    {
        private readonly string _sourceFile;
        private readonly long _startPosition;
        public  event DataProcessEventHandler ReadedEvent;
        private static readonly Semaphore SemRead = new Semaphore(Constants.ThreadCount , Constants.ThreadCount);

        public BlockReader(string sourceFile, long startPosition)
        {
            _sourceFile = sourceFile;
            _startPosition = startPosition;
        }

        public Thread ReadBlock(object inputObject)
        {
            var thread = new Thread(ReadBlockForLength);
            thread.Start(inputObject);
            return thread;
        }

        private void ReadBlockForLength(object inputObject)
        {
            SemRead.WaitOne();
            var dataInput = (Data)inputObject;
            var index = dataInput.ArrayIndex;
            var length = dataInput.Length == 0 ? (int) Constants.BlockLength : dataInput.Length;
            var block = new byte[length];
            using (var sourceStream = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read,
                FileShare.Read, (int)Constants.BlockLength, true))
            
            {
                sourceStream.Seek(_startPosition, SeekOrigin.Begin);
                sourceStream.Read(block, 0, length);
                ReadedEvent?.Invoke(this, new DataProcessEventArgs(block, index));
            }
            SemRead.Release();
        }
    }
}
