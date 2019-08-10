using System;

namespace GZipper
{
    public static class Constants
    {
        public const long BlockLength = 1024 * 1024;
        public const long BufferLength = 1024 * 4;
        public const int BigBlockCount =  1024;
        public const string Compress = "compress";
        public const string Decompress = "decompress";
        public static readonly int ThreadCount = Environment.ProcessorCount;
        public const int NumBytesAtCount = 4;
        public const string Separator = "Ё!~";
    }

    public enum Work 
    {
        Zip,
        Unzip
    }
}
