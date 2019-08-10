namespace GZipper
{
    public class Data
    {
        public byte[] Array;
        public readonly Work Action;
        public readonly int ArrayIndex;


        public Data(byte[] array, Work action, int arrayIndex)
        {
            Array = array;
            Action = action;
            ArrayIndex = arrayIndex;
        }
    }
}
