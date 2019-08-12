using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipper
{
    public delegate void ZippedEventHandler(object sender, ZippingEventArgs e);

    public interface IBlockZipper
    {
        Thread TreatBlock(object blockObj);
        event ZippedEventHandler ZippedEvent;
    }

    public class BlockZipper : IBlockZipper
    {
        private static readonly Semaphore SemZip = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);
        public event ZippedEventHandler ZippedEvent;


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
}