using System;

namespace GZipper
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = 0;
            try
            {
                ArgsHelper.CheckArgs(args);
                var zipAction = args?[0]?.ToLower();
                var sourceFile = args?[1]?.RemoveInvalidChars();
                var resultFile = args?[2]?.RemoveInvalidChars(); 

                IZipper zipper = new MyGZipper(sourceFile, resultFile);
                    switch (zipAction)
                    {
                        case "compress":
                            result = zipper.Zip();
                            break;
                        case "decompress":
                            if (sourceFile != null && !sourceFile.Contains(".gz"))
                            {
                                throw new Exception("Не верный формат файла архива");
                            }
                            result = zipper.Unzip();
                            break;
                    }
              
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine(@"Недостаточно оперативной памяти");
                result = 1;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }

            Console.WriteLine(result);
        }
    }
}
