namespace GZipper
{
    public delegate void DataProcessEventHandler(object sender, DataProcessEventArgs e);
    public delegate void ReadDataProcessEventHandler(object sender, ReadDataProcessEventArgs e);

    public class DataProcessEventArgs
    {
        public readonly byte[] DataProcessed;
        public readonly int IndexOfArray;
        public DataProcessEventArgs(byte[] dataProcessed, int indexOfArray)
        {
            DataProcessed = dataProcessed;
            IndexOfArray = indexOfArray;
        }
    }

    public class ReadDataProcessEventArgs
    {
        public readonly byte[][] DataProcessed;
        public readonly int IndexOfArray;
        public ReadDataProcessEventArgs(byte[][] dataProcessed, int indexOfArray)
        {
            DataProcessed = dataProcessed;
            IndexOfArray = indexOfArray;
        }
    }
}
