using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipper
{
    public static class Constants
    {
        public const long BlockLength = 1024 * 1024;
        public const string Compress = "compress";
        public const string Decompress = "decompress";
        public static readonly int ThreadCount = Environment.ProcessorCount;
    }

    public enum Work 
    {
        Zip,
        Unzip
    }
}
