using DatabaseConnectionLib;
using System;
using System.Windows.Controls;

namespace RestClient.Interfaces
{
    public class DishInBlock
    {
        private int _position;
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    throw new ArgumentException();

                _position = value;
            }
        }

        public Button CloseButton { get; }
        public Dish Dish { get; }
        public string Comment { get; set; }

        public DishInBlock(Button closeButton, Dish dish, string comment, int position)
        {
            CloseButton = closeButton;
            Dish = dish;
            Comment = comment;
            _position = position;
        }
    }
}