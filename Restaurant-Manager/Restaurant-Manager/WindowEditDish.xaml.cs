using DatabaseConnectionLib;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Restaurant_Manager
{
    public partial class WindowEditDish : Window
    {
        private Dish oldDish;

        public IEnumerable Categories
        {
            get { return comboBoxCategory.ItemsSource; }
            set { comboBoxCategory.ItemsSource = value; }
        }

        private string pathToImage;
        private bool IsCustomCategory = false;

        public WindowEditDish(Dish dish)
        {
            InitializeComponent();
            oldDish = dish;

            ImageSourceConverter imgsc = new ImageSourceConverter();
            image.Source = (ImageSource)imgsc.ConvertFrom(dish.PhotoPath);
            pathToImage = dish.PhotoPath;
            textBoxName.Text = dish.Name;
            textBoxDescription.Text = dish.Description;
            textBoxPrice.Text = dish.Price.ToString();
        }

        // Внесение изменений блюда в базу данных
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataIsValid)
            {
                DBConnector.RemoveDishAtName(oldDish.Name);
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
            else
            {
                backgroundBtnEdit.Visibility = Visibility.Visible;
                borderBtnEdit.IsEnabled = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
            => Close();

        // Проверка правильности заполнения полей
        #region Check Valid Data

        private bool DataIsValid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(textBoxPrice.Text)
                    && !string.IsNullOrWhiteSpace(textBoxName.Text)
                    && pathToImage != null
                    && (!IsCustomCategory && !string.IsNullOrWhiteSpace(comboBoxCategory.Text)
                        || IsCustomCategory && !string.IsNullOrWhiteSpace(textBoxCustomCategory.Text)))
                {
                    if (!(textBoxPrice.Text == oldDish.Price.ToString() && textBoxName.Text == oldDish.Name &&
                          textBoxDescription.Text == oldDish.Description && pathToImage == oldDish.PhotoPath &&
                          (!IsCustomCategory && comboBoxCategory.Text == oldDish.Category || 
                           IsCustomCategory && textBoxCustomCategory.Text == oldDish.Category))) return true;
                }
                return false;
            }
        }

        private readonly Regex priceRegex = new Regex(@"^[0-9]$");

        private void TextBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = priceRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataIsValid)
            {
                borderBtnEdit.IsEnabled = true;
                backgroundBtnEdit.Visibility = Visibility.Hidden;
            }
            else
            {
                borderBtnEdit.IsEnabled = false;
                backgroundBtnEdit.Visibility = Visibility.Visible;
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
                ImageSourceConverter imgsc = new ImageSourceConverter();
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    image.Source = (ImageSource)imgsc.ConvertFrom(files[0]);
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
                ImageSourceConverter imgsc = new ImageSourceConverter();
                try
                {
                    image.Source = (ImageSource)imgsc.ConvertFrom(ofd.FileName);
                    pathToImage = ofd.FileName;
                    TextChanged(this, null);
                }
                catch { }
            }

        }

        #endregion

        // Визуализация незаполненных полей при наведении на кнопку "Изменить"
        #region Visual Invalid Data

        private void btnEdit_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((textBoxPrice.Text == oldDish.Price.ToString() && textBoxName.Text == oldDish.Name &&
                 textBoxDescription.Text == oldDish.Description && pathToImage == oldDish.PhotoPath &&
                 (!IsCustomCategory && comboBoxCategory.Text == oldDish.Category ||
                  IsCustomCategory && textBoxCustomCategory.Text == oldDish.Category)))
                backgroundBtnEdit.ToolTip = "Для изменения блюда в базе данных нужно внести хоть одно изменение здесь";

                if (pathToImage == null) borderImage.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(textBoxName.Text)) borderTextBoxName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(comboBoxCategory.Text) && !IsCustomCategory
                || string.IsNullOrWhiteSpace(textBoxCustomCategory.Text) && IsCustomCategory) borderComboBoxCategory.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (string.IsNullOrWhiteSpace(textBoxPrice.Text)) borderTextBoxPrice.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void btnEdit_MouseLeave(object sender, MouseEventArgs e)
        {
            backgroundBtnEdit.ToolTip = null;
            borderImage.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderTextBoxName.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderComboBoxCategory.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderTextBoxPrice.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
        }

        #endregion

        // Добавление новой категории
        private void btnCustomCategory_Click(object sender, RoutedEventArgs e)
        {
            if (IsCustomCategory)
            {
                IsCustomCategory = false;
                comboBoxCategory.Visibility = Visibility.Visible;
                textBoxCustomCategory.Visibility = Visibility.Hidden;
                btnCustomCategory.Content = "+";
            }
            else
            {
                IsCustomCategory = true;
                comboBoxCategory.Visibility = Visibility.Hidden;
                textBoxCustomCategory.Visibility = Visibility.Visible;
                btnCustomCategory.Content = "-";
            }
            textBoxCustomCategory.Text = String.Empty;
        }
    }
}
