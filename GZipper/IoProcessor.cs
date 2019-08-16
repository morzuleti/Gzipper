namespace GZipper
{
    public interface IIoProcessor
    {
        void ReadBlocks(int numCurrentBigBlock, int numberOfBlocks, int[] blocksLength = null);
        void WriteFile(byte[][] blocks);
        event ReadDataProcessEventHandler ReadedEvent;
    }

    public class IoProcessor: IIoProcessor
    {
        private static string _sourceFile;
        private static string _destFile;
        private static int _countBigBlocks;
        private static long _totalLength;
        public event ReadDataProcessEventHandler ReadedEvent;

        public IoProcessor(string sourceFile, string destFile, int countBigBlocks, long totalLength)
        {
            _sourceFile = sourceFile;
            _destFile = destFile;
            _countBigBlocks = countBigBlocks;
            _totalLength = totalLength;
        }

        public void ReadBlocks(int numCurrentBigBlock, int numberOfBlocks, int[] blocksLength = null)
        {
            long currentBlockToSkip = numCurrentBigBlock * Constants.BigBlockCount;
            long currentPosition = 0;
            for (var i = 0; i < currentBlockToSkip; i++)
            {
                currentPosition += blocksLength?[i] ?? Constants.BlockLength;
            }

            var blocks = new byte[numberOfBlocks][];
            for (long i = 0; i < numberOfBlocks; i++)
            {
                var reader = new BlockReader(_sourceFile, currentPosition);
                var lastBlockLength = (int)(numCurrentBigBlock == _countBigBlocks && i == numberOfBlocks - 1
                    ? _totalLength % Constants.BlockLength
                    : 0);
                blocks[i] = reader.ReadBlock(blocksLength?[i + currentBlockToSkip] ?? lastBlockLength);
                currentPosition += blocks[i].Length;
            }
            ReadedEvent.Invoke(this, new ReadDataProcessEventArgs(blocks, numCurrentBigBlock));
        }

        public void WriteFile(byte[][] blocks)
        {
            foreach (var block in blocks)
            {
                var blockWriter = new BlockWriter(_destFile);
                blockWriter.WriteBlock(block);
            }
        }
    }
}
