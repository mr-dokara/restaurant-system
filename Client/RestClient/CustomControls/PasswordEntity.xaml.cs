using System.Windows.Controls;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for PasswordEntity.xaml
    /// </summary>
    public partial class PasswordEntity : UserControl
    {
        private string _text;

        public string Text
        {
            get => (string)PassEntity.Content;
            private set => _text = (string)PassEntity.Content;
        }
        public PasswordEntity()
        {
            InitializeComponent();
        }
    }
}
