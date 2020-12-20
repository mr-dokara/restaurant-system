using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficiantLib
{
    public class Officiant
    {
        public string Name { get; set; }

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

        public Officiant() { }

        public Officiant(string name)
        {
            Name = name;
            CountOfOrders = 0;
            CleanProfit = 0;
        }
    }

    public class Dish
    {
        public string Name { get; set; }

        private float _price;
        public float Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException();

                _price = value;
            }
        }

        private int _count;
        public int Count
        {
            get => _count;
            set
            {
                if (value < 1)
                    throw new ArgumentException();

                _count = value;
            }
        }

        public string Comment { get; set; }

        public Dish() { }

        public Dish(string name, float price, int count = 1)
        {
            Name = name;
            Price = price;
            Count = count;
        }
    }

    public class Order
    {
        public ICollection<Dish> Dishes { get; set; }
        public int TableIndex { get; set; }

        public Order()
        {
            Dishes = new List<Dish>();
        }

        public Order(IEnumerable<Dish> dishes, int tableIndex) : this()
        {
            foreach (var dish in dishes)
            {
                Dishes.Add(dish);
            }

            TableIndex = tableIndex;
        }

        public void RemoveByName(string name)
        {
            foreach (var dish in Dishes.Where(dish => dish.Name == name))
            {
                Dishes.Remove(dish);
                return;
            }
        }

        private bool Remove(Dish item)
        {
            return Dishes.Remove(item);
        }
    }

    public class OrderData
    {
        public Officiant Officiant { get; set; }
        public Order Order { get; set; }
        public DatabaseConnectionLib.Order DbOrder { get; set; }

        public OrderData()
        { }

        public OrderData(Officiant officiant, Order order, DatabaseConnectionLib.Order dbOrder)
        {
            Officiant = officiant;
            Order = order;
            DbOrder = dbOrder;
        }
    }
}
