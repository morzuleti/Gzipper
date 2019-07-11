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
        private const int BlockLength = 1024 * 1024;

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
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate, FileAccess.Read,
                FileShare.Read, 4048, true))
            {
                var lenth = sourceStream.Length;
                var countBlocks = lenth >> 20;
                
                byte[][] blocks = new byte[++countBlocks][];
                for (int i = 0; i < blocks.Length-1; i++)
                {
                    blocks[i] = new byte[BlockLength];
                    sourceStream.Seek(i * BlockLength, SeekOrigin.Begin);
                    sourceStream.Read(blocks[i], 0, BlockLength);
                }

                int lastBlockLength = (int)sourceStream.Length % BlockLength;
                blocks[countBlocks-1] = new byte[lastBlockLength];
                sourceStream.Read(blocks[countBlocks-1], 0, lastBlockLength);
                return blocks;
            }
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
