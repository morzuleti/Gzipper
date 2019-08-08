namespace GZipper
{
    public class ZippingEventArgs
    {
        public readonly byte[] ZippedBytes;
        public readonly int IndexOfArray;
        public ZippingEventArgs(byte[] zippedBytes, int indexOfArray)
        {
            ZippedBytes = zippedBytes;
            IndexOfArray = indexOfArray;
        }
    }
}
