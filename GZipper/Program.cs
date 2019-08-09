using System;

namespace GZipper
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[3];
            args[0] = Constants.Decompress;
            args[1] = @"D:\\test\Hardstyle.gz";
            args[2] = @"D:\\test\Hardstyle1.mp4";
#endif

            if (args.Length < 1)
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
