using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipper
{
    public static class Constants
    {
        public static readonly int BlockLength = 1024 * 1024;
        public static readonly int ThreadCount = Environment.ProcessorCount;
    }
}
