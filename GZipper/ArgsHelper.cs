using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipper
{
    public static class ArgsHelper
    {
        public static void CheckArgs(string[] args)
        {
            if (args == null || args.Length != 3)
            {
                throw new Exception("Укажите аргументы: compress/decompress file1 file2");
            }
            string zipAction = args[0].ToLower();
            if (!(zipAction.Equals(Constants.Compress, StringComparison.OrdinalIgnoreCase)
                  || zipAction.Equals(Constants.Decompress, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Укажите действие: compress/decompress");
            }

            if (!ValidateFileName(args[1]) || !ValidateFileName(args[2], false))
            {
                throw new Exception("Выход из программы в связи с некорректными аргументами");
            }

        }

        public static bool ValidateFileName(string name, bool isSource = true)
        {
            try
            {
                var fs = File.Open(name, isSource ? FileMode.Open : FileMode.Create);
                fs.Close();
            }
            catch (ArgumentException)
            {
                Console.WriteLine(@"Имя файла {0} введено некорректно", name);
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(@"Файл {0} не существует", name);
                return false;
            }
            catch (IOException)
            {
                Console.WriteLine(@"Файл {0} не доступен", name);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(@"Доступ к файлу {0} запрещен", name);
                return false;
            }

            return true;
        }

        public static string RemoveInvalidChars(this string fileName)
        {
            return Path.GetInvalidPathChars().Aggregate(fileName, (current, invalidChar) =>
                current.Replace(oldValue: invalidChar.ToString(), newValue: ""));
        }
    }
}
