namespace Hake.Extension.Cache
{
    internal sealed class CounterValue
    {
        private int counter;

        public int Counter => counter;

        public void AddCounter()
        {
            counter++;
        }

        public void ResetCounter()
        {
            this.counter = 0;
        }
    }
}
