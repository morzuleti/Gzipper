using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace GZipper
{


    public interface IBlockZipper
    {
        MemoryStream ZipBlock(byte[] blocks);
        MemoryStream UnZipBlock(byte[] blocks);
    }

    public class BlockZipper : IBlockZipper
    {
        private static readonly Semaphore SemZip = new Semaphore(Constants.ThreadCount, Constants.ThreadCount);

        public MemoryStream ZipBlock(byte[] blocks)
        {


            return Ziper(new Data(blocks, Work.Zip));

        }

        public MemoryStream UnZipBlock(byte[] blocks)
        {
            return Ziper(new Data(blocks, Work.Unzip));
        }

        private MemoryStream Ziper (Data blockObj)
        {
            var output = blockObj.Action == Work.Zip ? ZipIt(blockObj) : UnzipIt(blockObj);
            return output;
        }

        private static MemoryStream UnzipIt(Data data)
        {
            using (var decompressionStream = new GZipStream(new MemoryStream(data.Array, 0, data.Array.Length), CompressionMode.Decompress))
            {

                byte[] buffer = new byte[Constants.BlockLength];
                using (MemoryStream memory = new MemoryStream())
                {
                    var count = 0;
                    do
                    {
                        count = decompressionStream.Read(buffer, 0, buffer.Length);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                  return  memory;
                }
            }

        }

        private static MemoryStream ZipIt(Data data)
        {
            SemZip.WaitOne();

            MemoryStream output = new MemoryStream();
            using (GZipStream cs = new GZipStream(output, CompressionMode.Compress, false))
            {
                cs.Write(data.Array, 0, data.Array.Length);
            }
            SemZip.Release();

            return output;
        }
    }

    public class Data
    {
        public readonly byte[] Array;
        public readonly Work Action;


        public Data(byte[] array, Work action)
        {
            Array = array;
            Action = action;
        }
    }
}