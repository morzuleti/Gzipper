using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipper
{
    public class BlockZipper
    {
        private byte[][] _blocks;
        private static readonly Semaphore Sem = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);

        public BlockZipper(byte[][] blocks)
        {
            _blocks = blocks;
        }

        public void ZipBlocks()
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                var myThread = new Thread(Ziper);
                myThread.Start(new Obertka(_blocks[i], i, Work.Zip));
                myThread.Join(Int32.MaxValue);
            }
        }

        public void UnZipBlocks()
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                var myThread = new Thread(Ziper);
                myThread.Start(new Obertka(_blocks[i], i, Work.Unzip));
            }
        }

        private void Ziper(object blockObj)
        {
            Sem.WaitOne();
            var obertka = (Obertka)blockObj;
            using (MemoryStream output = new MemoryStream(obertka.Array.Length))
            {
                if (obertka.Action == Work.Zip)
                {
                    ZipIt(output, obertka);
                }
                else
                {
                    UnzipIt(output, obertka);
                }

                _blocks[obertka.Index] = output.ToArray();
            }
            Sem.Release();
        }

        private static void UnzipIt(MemoryStream output, Obertka obertka)
        {
            using (GZipStream cs = new GZipStream(output, CompressionMode.Decompress))
            {
                cs.Read(obertka.Array, 0, obertka.Array.Length);
            }
        }

        private static void ZipIt(MemoryStream output, Obertka obertka)
        {
            using (GZipStream cs = new GZipStream(output, CompressionMode.Compress))
            {
                cs.Write(obertka.Array, 0, obertka.Array.Length);
            }
        }
    }

    public class Obertka
    {
        public readonly byte[] Array;
        public readonly int Index;
        public readonly Work Action;


        public Obertka(byte[] array, int index, Work action)
        {
            Array = array;
            Index = index;
            Action = action;
        }
    }
}