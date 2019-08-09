using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipper
{
    class BlockWriter
    {
        // создаем семафор
        private static List<int> _endBytes = new List<int>();
        private static string _destFile;
        private readonly int _blockPoz;

        public BlockWriter(string destFile, int blockPoz)
        {
            _blockPoz = blockPoz;
            _destFile = destFile;
        }

        public void WriteBlock(byte[] block)
        {
            Write(block);
        }

        private void Write(byte[] block)
        {
            using (var sourceStream = new FileStream(_destFile, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.Write, 4048, false))
            {
                sourceStream.Seek(_blockPoz, SeekOrigin.Begin);
                sourceStream.Write(block, 0, block.Length);
                _endBytes.Add(block.Length);
            }
        }

        public static void FinalizeFile()
        {
            using (var finalizeStream = new FileStream(_destFile, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.Write, 4048, false))
            {
                var delimiter = Encoding.UTF8.GetBytes(Constants.Separator).ToList();
                foreach (var val in _endBytes)
                {
                    List<byte> valToAppend = new List<byte>();
                    valToAppend.AddRange(delimiter);
                    valToAppend.AddRange(Encoding.UTF8.GetBytes(val.ToString()));

                    finalizeStream.Seek(0, SeekOrigin.End);

                    finalizeStream.Write(valToAppend.ToArray(), 0, valToAppend.Count);
                }
                var countBlocks = BitConverter.GetBytes(_endBytes.Count);
                finalizeStream.Seek(0, SeekOrigin.End);
                finalizeStream.Write(countBlocks, 0, countBlocks.Length);
            }
        }
    }
}