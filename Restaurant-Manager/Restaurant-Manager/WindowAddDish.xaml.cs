using DatabaseConnectionLib;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Restaurant_Manager
{
    public partial class WindowAddDish : Window
    {
        public IEnumerable<Dish> LocalDishes;

        public IEnumerable Categories
        {
            get { return comboBoxCategory.ItemsSource; }
            set { comboBoxCategory.ItemsSource = value; }
        }

        private string pathToImage;

        public WindowAddDish()
        {
            InitializeComponent();
        }

        // Добавление нового блюда в базу данных
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            textBoxName.Text = textBoxName.Text.Trim();

            if (DataIsValid)
            {
                DBConnector.AddDish(new Dish
                {
                    Name = textBoxName.Text,
                    Category = comboBoxCategory.Text,
                    Description = textBoxDescription.Text,
                    Price = float.Parse(textBoxPrice.Text),
                    PhotoPath = pathToImage
                });
                DialogResult = true;
                Close();
            }
            else borderBtnAdd.IsEnabled = false;
        }

        // Проверка правильности заполнения полей
        #region Check Valid Data

        private bool DataIsValid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(textBoxPrice.Text) && long.Parse(textBoxPrice.Text) > 0
                    && !string.IsNullOrWhiteSpace(textBoxName.Text) && textBoxName.Text.Length > 1
                    && LocalDishes.FirstOrDefault(dish => dish.Name == textBoxName.Text) == null
                    && pathToImage != null
                    && !string.IsNullOrWhiteSpace(comboBoxCategory.Text))
                    return true;
                return false;
            }
        }

        private readonly Regex priceRegex = new Regex(@"[0-9]");

        private void TextBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = priceRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private readonly Regex wordsRegex = new Regex(@"[а-яА-Я\ ]");

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = wordsRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                if ((sender as TextBox).Name == "textBoxPrice")
                {
                    MatchCollection matches;
                    if (!string.IsNullOrWhiteSpace(textBoxPrice.Text) &&
                        (matches = priceRegex.Matches(textBoxPrice.Text.Replace(" ", ""))).Count !=
                        textBoxPrice.Text.Replace(" ", "").Length)
                    {
                        textBoxPrice.Text = string.Empty;
                        foreach (Match match in matches)
                        {
                            if (textBoxPrice.Text.Length > 20) break;
                            textBoxPrice.Text += match.Value;
                        }
                    }
                }

                if ((sender as TextBox).Name == "textBoxName")
                {
                    MatchCollection matches;
                    if (!string.IsNullOrWhiteSpace(textBoxName.Text) &&
                        (matches = wordsRegex.Matches(textBoxName.Text.Replace(" ", ""))).Count !=
                        textBoxName.Text.Replace(" ", "").Length)
                    {
                        textBoxName.Text = string.Empty;
                        foreach (Match match in matches)
                        {
                            textBoxName.Text += match.Value;
                        }

                        textBoxName.Text = textBoxName.Text.Trim();
                    }
                }
            }

            if (DataIsValid)
            {
                borderBtnAdd.IsEnabled = true;
                backgroundBtnAdd.Visibility = Visibility.Hidden;
            }
            else
            {
                borderBtnAdd.IsEnabled = false;
                backgroundBtnAdd.Visibility = Visibility.Visible;
            }
        }

        private void ComboBoxCategory_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            => TextChanged(sender, null);

        private void TextBoxPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true;
        }

        #endregion

        // Система Drag`n`drop для изображения и диалог на прямое добавление изображения
        #region Change Image

        private void Image_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    image.Source = ImageExtention.GetImageSourceFromFile(files[0]);
                    pathToImage = files[0];
                    TextChanged(this, null);
                }
                catch { }
            }
        }

        private void Image_OnClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    image.Source = ImageExtention.GetImageSourceFromFile(ofd.FileName);
                    pathToImage = ofd.FileName;
                    TextChanged(this, null);
                }
                catch { }
            }

        }

        #endregion

        // Визуализация незаполненных полей при наведении на кнопку "Добавить"
        #region Visual Invalid Data

        private void btnAdd_MouseEnter(object sender, MouseEventArgs e)
        {
            if (pathToImage == null) borderImage.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(textBoxName.Text) || textBoxName.Text.Length <= 1 || LocalDishes.FirstOrDefault(dish => dish.Name == textBoxName.Text) != null) borderTextBoxName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(comboBoxCategory.Text)) borderComboBoxCategory.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(textBoxPrice.Text) || long.Parse(textBoxPrice.Text) <= 0) borderTextBoxPrice.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void btnAdd_MouseLeave(object sender, MouseEventArgs e)
        {
            borderImage.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderTextBoxName.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderComboBoxCategory.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderTextBoxPrice.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
        }

        #endregion
    }
}
