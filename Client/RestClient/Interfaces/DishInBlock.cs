using System.Windows.Controls;
using DatabaseConnectionLib;

namespace RestClient.Interfaces
{
    public class DishInBlock
    {
        private int _position;
        private Button _closeButton;
        private Dish _dish;

        public DishInBlock(Button closeButton, Dish dish, int position)
        {
            _closeButton = closeButton;
            _dish = dish;
        }
    }
}