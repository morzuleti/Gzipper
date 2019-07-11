using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace GZipper
{
    class Program
    {
        static void Main(string[] args)
        {
            string zip = args[0].ToLower();
            if (!(zip == Constants.Compress || zip == Constants.Decompress))
            {
                Console.WriteLine("Не верный способ обработки файла!");
                return;
            }

            string sourceFile = args[1]; // исходный файл
            string resultFile = args[2]; // выходной файл 

            IZipper zipper = new MyGZipper(sourceFile, resultFile);
            var result = 0;
            switch (zip)
            {
                case "Compress":
                    result = zipper.Zip();
                    break;
                case "Decompress":
                    result = zipper.Unzip();
                    break;
            }
            Console.WriteLine(result);
            Console.ReadLine();
        }

        //private static byte[][] Read(string sourceFile)
        //{
        //    long countBlocks;
        //    long length;
        //    using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
        //    {
        //        length = sourceStream.Length;
        //        countBlocks = length >> 20;
        //    }
        //    byte[][] blocks = new byte[countBlocks + 1][];

        //    int lastBlockLength = (int)(length % Constants.BlockLength);
        //    blocks[countBlocks] = new byte[lastBlockLength];
        //    var abdc = new BlockReader(sourceFile, (int)countBlocks, lastBlockLength);
        //    abdc.ReadBlock(blocks[countBlocks]);

        //    for (int i = 0; i < countBlocks; i++)
        //    {
        //        blocks[i] = new byte[Constants.BlockLength];
        //        var abc = new BlockReader(sourceFile, i, Constants.BlockLength);
        //        abc.ReadBlock(blocks[i]);
        //    }
        //    return blocks;
        //}

        //private static int Write(string compressedFile, byte[][] sourceBytes)
        //{
        //    using (FileStream targetStream = File.OpenWrite(compressedFile))
        //    {
        //        using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
        //        {
        //            foreach (var block in sourceBytes)
        //            {
        //                compressionStream.Write(block, 0, block.Length);
        //            }
        //        }
        //    }

        //    return 0;
        //}
    }
}
