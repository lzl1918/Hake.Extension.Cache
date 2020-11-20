namespace Hake.Extension.Cache
{
    internal sealed class Counter
    {
        private int mCount;

        public int Count => mCount;

        public void Increase()
        {
            mCount++;
        }

        public void Reset()
        {
            mCount = 0;
        }
    }
}
