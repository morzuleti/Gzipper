using System;
using System.IO;

namespace GZipper
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[3];
            args[0] = Constants.Decompress;
            args[1] = @"D:\\test\cyborg.gz";
            args[2] = @"D:\\test\cyborg1.config";
#endif

            if (args.Length < 3)
            {
                Console.WriteLine("compress/decompress file1 file2 args needed");
                Console.ReadKey();
            }

            string zipAction = args[0].ToLower();
            if (!(zipAction.Equals(Constants.Compress, StringComparison.OrdinalIgnoreCase) 
                  || zipAction.Equals(Constants.Decompress, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("compress/decompress needed");
                return;
            }

            string sourceFile = args[1]; // исходный файл
            string resultFile = args[2]; // выходной файл 

            IZipper zipper = new MyGZipper(sourceFile, resultFile);
            var result = 0;
            try
            {
                switch (zipAction)
                {
                    case "compress":
                        result = zipper.Zip();
                        break;
                    case "decompress":
                        result = zipper.Unzip();
                        break;
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Console.WriteLine(@"Недостаточно оперативной памяти");
                Console.WriteLine(outOfMemoryException.Message);
                result = 1;
            }
            catch (IOException ioException)
            {
                Console.WriteLine(@"Ошибка чтения/записи");
                Console.WriteLine(ioException.Message);
                result = 1;
            }
            catch (UnauthorizedAccessException auth)
            {
                Console.WriteLine(@"Доступ к файлу {0} запрещен", sourceFile);
                Console.WriteLine(auth.Message);
                result = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }

            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
