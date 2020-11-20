using System.Windows;
using System.Windows.Controls;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for BlockView.xaml
    /// </summary>
    public partial class BlockView : UserControl
    {
        private RoutedEventHandler _handler;
        public BlockView()
        {
            InitializeComponent();
        }

        public BlockView(RoutedEventHandler action) : this()
        {
            _handler = action;
            CancelButton.Click += action;
        }

        public Button CloseButton => CancelButton;

        public string Caption
        {
            get => CaptionLabel.Content.ToString();
            set => CaptionLabel.Content = value;
        }

        public string Price
        {
            get => PriceLabel.Content.ToString();
            set => PriceLabel.Content = value;
        }

        public void SetEventToCancelButton(RoutedEventHandler action)
        {
            if (_handler != null)
                CancelButton.Click -= _handler;

            CancelButton.Click += action;
            _handler = action;
        }
    }
}
