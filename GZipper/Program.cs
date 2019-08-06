using System;

namespace GZipper
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[3];
            args[0] = Constants.Decompress;
            args[1] = @"D:\\test\book.gz";
            args[2] = @"D:\\test\book1.pdf";

            if (args.Length < 1)
            {
                Console.WriteLine("compress/decompress file1 file2 args needed");
                Console.ReadKey();
            }

            string zip = args[0].ToLower();
            if (!(zip == Constants.Compress || zip == Constants.Decompress))
            {
                Console.WriteLine("compress/decompress needed");
                return;
            }

            string sourceFile = args[1]; // исходный файл
            string resultFile = args[2]; // выходной файл 

            IZipper zipper = new MyGZipper(sourceFile, resultFile);
            var result = 0;
            switch (zip)
            {
                case "compress":
                    result = zipper.Zipping();
                    break;
                case "decompress":
                    result = zipper.Unzip();
                    break;
            }
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
