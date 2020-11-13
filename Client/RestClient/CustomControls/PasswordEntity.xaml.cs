using RestClient.Interfaces;
using System.ComponentModel;
using System.Windows.Controls;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for PasswordEntity.xaml
    /// </summary>
    public partial class PasswordEntity : UserControl, ITextView
    {
        public delegate void Authentication(string pass);
        public event Authentication FourCharactersEntered;

        [Description("Hide password or not"), Category("Data")]
        public bool IsHidden { get; set; } = false;

        [Description("A character that replaces numbers in a password."), Category("Data")]
        public char HidingSymbol { get; set; } = '•';

        public string Text
        {
            get => (string)PassEntity.Content;
            set
            {
                PassEntity.Content = value;
                if (PassEntity.Content.ToString().Length != 4)
                    FourCharactersEntered?.Invoke(PassEntity.Content.ToString());
            }
        }

        public PasswordEntity()
        {
            InitializeComponent();
        }
    }
}