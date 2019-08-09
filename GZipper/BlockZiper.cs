using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipper
{


    public interface IBlockZipper
    {
        Thread TreatBlock(object blockObj);
    }

    public class BlockZipper : IBlockZipper
    {
        private static readonly Semaphore SemZip = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        public delegate void SampleEventHandler(object sender, ZippingEventArgs e);
        public event SampleEventHandler ZippedEvent;


        public Thread TreatBlock(object blockObj)
        {
            Thread myThread = new Thread(Zipper);
            myThread.Start(blockObj);
            return myThread;
        }

        private void Zipper (object blockObj)
        {
            SemZip.WaitOne();
            var data = (Data)blockObj;
            data.Array = data.Action == Work.Zip ? ZipIt(data) : UnzipIt(data);
            SemZip.Release();
            ZippedEvent?.Invoke(this, new ZippingEventArgs(data.Array, data.ArrayIndex) );
        }

        private static byte[] UnzipIt(Data data)
        {
            using (var decompressionStream = new GZipStream(new MemoryStream(data.Array, 0, data.Array.Length), CompressionMode.Decompress))
            {

                using (MemoryStream memory = new MemoryStream())
                {
                    decompressionStream.CopyTo(memory);
                    return  memory.ToArray();
                }
            }

        }

        private static byte [] ZipIt(Data data)
        {
            MemoryStream output = new MemoryStream();
            using (GZipStream cs = new GZipStream(output, CompressionMode.Compress, false))
            {
                cs.Write(data.Array, 0, data.Array.Length);
            }
            return output.ToArray();
        }
    }

    public class Data
    {
        public byte[] Array;
        public readonly Work Action;
        public int ArrayIndex;


        public Data(byte[] array, Work action, int arrayIndex)
        {
            Array = array;
            Action = action;
            ArrayIndex = arrayIndex;
        }
    }
}