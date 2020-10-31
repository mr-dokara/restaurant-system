using System.Windows.Controls;

namespace ClientOfficiant.User_Controls
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
