namespace GZipper
{
    public class Data
    {
        public byte[] Array;
        public readonly Work Action;
        public readonly int ArrayIndex;
        public readonly int Length;

        public Data(byte[] array, Work action, int arrayIndex, int length = 0)
        {
            Array = array;
            Action = action;
            ArrayIndex = arrayIndex;
            Length = length;
        }
    }
}
