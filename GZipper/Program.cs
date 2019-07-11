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
            string sourceFile = "D://test/book.pdf"; // исходный файл
            string compressedFile = "D://test/book.gz"; // сжатый файл
            string targetFile = "D://test/book_new.pdf"; // восстановленный файл

            Read(sourceFile);
            // создание сжатого файла
            Compress(sourceFile, compressedFile);
            // чтение из сжатого файла
            Decompress(compressedFile, targetFile);

            Console.ReadLine();
        }

        private static void Compress(string sourceFile, string compressedFile)
        {
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                // поток для записи сжатого файла
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                        Console.WriteLine("Сжатие файла {0} завершено. Исходный размер: {1}  сжатый размер: {2}.",
                            sourceFile, sourceStream.Length.ToString(), targetStream.Length.ToString());
                    }
                }
            }
        }

        private static byte[][] Read(string sourceFile)
        {
            long countBlocks;
            long length;
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                length = sourceStream.Length;
                countBlocks = length >> 20;
            }
            byte[][] blocks = new byte[countBlocks+1][];

            int lastBlockLength = (int)(length % Constants.BlockLength);
            blocks[countBlocks] = new byte[lastBlockLength];
            var abdc = new BlockReader(sourceFile, (int)countBlocks, lastBlockLength);
            abdc.ReadBlock(blocks[countBlocks]); 

            for (int i = 0; i < countBlocks; i++)
            {
                blocks[i] = new byte[Constants.BlockLength];
                var abc = new BlockReader(sourceFile, i, Constants.BlockLength);
                abc.ReadBlock(blocks[i]);
            }
            return blocks;
        }

        private static int Write(string compressedFile, byte[][] sourceBytes)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        memStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                    }
                }
            }

            return 0;
        }

        private static void Decompress(string compressedFile, string targetFile)
        {
            // поток для чтения из сжатого файла
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                // поток для записи восстановленного файла
                using (FileStream targetStream = File.Create(targetFile))
                {
                    // поток разархивации
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                        Console.WriteLine("Восстановлен файл: {0}", targetFile);
                    }
                }
            }
        }
    }
}
