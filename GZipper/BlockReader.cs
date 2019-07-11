using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipper
{
    class BlockReader
    {
        // создаем семафор
        static Semaphore sem = new Semaphore(3, 3);

        public BlockReader(char[] block)
        {
            var myThread = new Thread(new ParameterizedThreadStart(Read));
            myThread.Start(block);
        }

        private void Read(object block)
        {
            var block1 = (char[]) block;
                sem.WaitOne();

                sem.Release();
        }
    }
}
