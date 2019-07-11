using System;

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
                case "compress":
                    result = zipper.Zip();
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
