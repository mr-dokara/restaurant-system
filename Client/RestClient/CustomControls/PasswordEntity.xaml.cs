using System;
using System.Windows.Controls;
using RestClient.Interfaces;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for PasswordEntity.xaml
    /// </summary>
    public partial class PasswordEntity : UserControl, ITextView
    {
        public event EventHandler FourCharactersEntered;
        public string Text
        {
            get => (string)PassEntity.Content;
            set
            {
                PassEntity.Content = value;
                if (PassEntity.Content.ToString().Length != 4)
                    FourCharactersEntered?.Invoke(this, EventArgs.Empty);
            }
        }

        public PasswordEntity()
        {
            InitializeComponent();
        }
    }
}