using System.IO;

namespace GZipper
{
    class BlockReader
    {
        // создаем семафор
        private readonly string _sourceFile;
        private readonly long _position;

        public BlockReader(string sourceFile, long position)
        {
            _sourceFile = sourceFile;
            _position = position;
        }

        public byte[] ReadBlock(int length = 0)
        {
            return ReadBlockForLength(length == 0 ? (int)Constants.BlockLength : length);

        }

        private byte[] ReadBlockForLength(int length)
        {
            var block = new byte[length];
            using (var sourceStream = new FileStream(_sourceFile, FileMode.Open, FileAccess.Read,
                FileShare.Read, length, true))
            {
                sourceStream.Seek(_position, SeekOrigin.Begin);
                sourceStream.Read(block, 0, length);
            }

            return block;
        }
    }
}
