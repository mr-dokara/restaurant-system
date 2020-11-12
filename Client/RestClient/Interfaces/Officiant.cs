using System;

namespace RestClient.Interfaces
{
    public class Officiant
    {
        public string Name { get; }
        
        private int _countOfOrders;
        public int CountOfOrders
        {
            get => _countOfOrders;
            set => IntSetter(ref _countOfOrders, value);
        }

        private int _cleanProfit;
        public int CleanProfit
        {
            get => _cleanProfit;
            set => IntSetter(ref _cleanProfit, value);
        }

        private static void IntSetter(ref int toSet, int value)
        {
            if (value < 0)
                throw new ArgumentException();

            toSet = value;
        }

        public Officiant(string name)
        {
            Name = name;
            CountOfOrders = 0;
            CleanProfit = 0;
        }
    }
}