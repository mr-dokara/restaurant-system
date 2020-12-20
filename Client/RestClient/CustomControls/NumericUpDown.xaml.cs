using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public NumericUpDown()
        {
            InitializeComponent();
            txtNum.Text = _numValue.ToString();
        }

        private int _numValue = 1;

        public int NumValue
        {
            get => _numValue;
            set
            {
                if (value <= 0 || value > 100) throw new ArgumentException();
                _numValue = value;
                txtNum.Text = value.ToString();
            }
        }

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NumValue++;
            }
            catch (ArgumentException)
            {
                //do nothing
            }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NumValue--;
            }
            catch (ArgumentException)
            {
                //do nothing
            }
        }

        private void txtNum_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }

            var regex = new Regex(@"[0-9]");
            e.Handled = !regex.IsMatch(e.Text);

            int.TryParse(txtNum.Text, out var number);

            try
            {
                NumValue = number;
                txtNum.Text = NumValue.ToString();
            }
            catch (ArgumentException)
            {
                txtNum.Text = NumValue.ToString();
            }
        }
    }
}
