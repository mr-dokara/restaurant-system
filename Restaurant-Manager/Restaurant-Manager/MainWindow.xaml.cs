using DatabaseConnectionLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Restaurant_Manager
{
    public partial class MainWindow : Window
    {
        private DataType currentData = DataType.None;
        private object currentItem = null;

        private IEnumerable<Dish> localDishes;

        private bool IsLoading = true;
        

        public MainWindow()
        {
            InitializeComponent();
        }


        private void ShowTable(DataType type)
        {
            DataGridDishes.Visibility = Visibility.Hidden;
            listBoxPersonal.Visibility = Visibility.Hidden;

            btnAddNew.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            btnDeleteAll.IsEnabled = false;
            btnEdit.IsEnabled = false;
            btnDelete.IsEnabled = false;


            if (type == DataType.Personal)
            {
                currentData = DataType.Personal;
                labelNameTable.Content = "Список официантов";
            }

            if (type == DataType.Dishes)
            {
                currentData = DataType.Dishes;
                labelNameTable.Content = "Список блюд";
            }
            SyncDB();
        }


        // Методы для синхронизации с базой данных
        #region Synchronization

        private Task SynchronizationTask = null;
        private Timer refreshTimer;

        private void SyncDB()
        {
            if (SynchronizationTask == null || SynchronizationTask.IsCompleted)
            {
                refresh.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
                {
                    From = 0,
                    To = 720,
                    Duration = TimeSpan.FromSeconds(1)
                });

                refreshTimer = new Timer(1000);
                refreshTimer.AutoReset = true;
                refreshTimer.Elapsed += (e, a) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        refresh.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
                        {
                            From = 0,
                            To = 720,
                            Duration = TimeSpan.FromSeconds(1)
                        });
                    });
                };

                refreshTimer.Start();

                DataGridDishes.ItemsSource = null;
                listBoxPersonal.ItemsSource = null;

                btnDishes.IsEnabled = false;
                btnPersonal.IsEnabled = false;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;

                IsLoading = true;
                SynchronizationTask = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        if (currentData == DataType.Dishes)
                        {
                            var dbDishes = DBConnector.GetDishes();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                localDishes = dbDishes;
                                DataGridDishes.ItemsSource = dbDishes;

                                DataGridDishes.Visibility = Visibility.Visible;
                                btnAddNew.IsEnabled = true;
                                btnRefresh.IsEnabled = true;

                                if (dbDishes.Count() > 0) btnDeleteAll.IsEnabled = true;
                            });
                        }
                        else if (currentData == DataType.Personal)
                        {
                            var dbPersonal = DBConnector.GetOficiants();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                listBoxPersonal.ItemsSource = dbPersonal;

                                listBoxPersonal.Visibility = Visibility.Visible;
                                btnAddNew.IsEnabled = true;
                                btnRefresh.IsEnabled = true;

                                if (dbPersonal.Count() > 0) btnDeleteAll.IsEnabled = true;

                                btnDishes.IsEnabled = true;
                                btnPersonal.IsEnabled = true;
                            });
                        }
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            btnDishes.IsEnabled = true;
                            btnPersonal.IsEnabled = true;

                            refreshTimer.Stop();
                            refreshTimer.Close();
                        });
                        IsLoading = false;
                    }
                });

            }
        }

        private bool IsSynchronized
        {
            get
            {
                if (currentData == DataType.Dishes)
                {
                    var dbDishes = DBConnector.GetDishes();
                    var dishes = DataGridDishes.ItemsSource.Cast<Dish>();

                    foreach (Dish dish in dishes)
                    {
                        Dish currDish;
                        if ((currDish = dbDishes.FirstOrDefault(x => x.Name == dish.Name)) == null ||
                            currDish.Category != dish.Category || currDish.Description != dish.Description ||
                            currDish.Price != dish.Price) return false;
                    }

                    foreach (Dish dish in dbDishes)
                    {
                        Dish currDish;
                        if ((currDish = dishes.FirstOrDefault(x => x.Name == dish.Name)) == null ||
                            currDish.Category != dish.Category || currDish.Description != dish.Description ||
                            currDish.Price != dish.Price) return false;
                    }

                    return true;
                }

                if (currentData == DataType.Personal)
                {
                    var dbPersonal = DBConnector.GetOficiants();
                    var Personal = listBoxPersonal.ItemsSource.Cast<string>();

                    foreach (string oficiant in Personal)
                    {
                        if (dbPersonal.FirstOrDefault(x => x == oficiant) == null) return false;
                    }

                    foreach (string oficiant in dbPersonal)
                    {
                        if (Personal.FirstOrDefault(x => x == oficiant) == null) return false;
                    }

                    return true;
                }
                return false;
            }
        }

        #endregion


        // Методы, вызываемые событиями нажатия кнопок на панели выбора таблицы (верхняя горизонтальная панель)
        #region Select Table Events

        private void OpenPersonalMenu(object sender, RoutedEventArgs e)
            => ShowTable(DataType.Personal);

        private void OpenDishesMenu(object sender, RoutedEventArgs e)
            => ShowTable(DataType.Dishes);

        #endregion


        // Методы, вызываемые событиями после нажатия кнопок на панели инструментов (правая вертикальная панель)
        #region Tool Panel Events

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить все записи?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (currentData == DataType.Dishes)
                {
                    // Полезная функция, но использовать её пожалуй не стоит
                    // DBConnector.ClearDishes();
                    // SyncDB();
                }
                else if (currentData == DataType.Personal)
                {
                    var table = DBConnector.GetOficiants();
                    foreach (string login in table)
                    {
                        DBConnector.RemoveOficiant(login);
                    }

                    table = DBConnector.GetOficiants();
                    var tablePersonal = new List<string>(table.Count());
                    foreach (string s in table)
                    {
                        tablePersonal.Add(s);
                    }

                    listBoxPersonal.ItemsSource = tablePersonal;
                }

                btnDeleteAll.IsEnabled = false;
            }
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (currentData == DataType.Dishes)
            {
                var window = new WindowAddDish();
                window.Owner = this;
                window.LocalDishes = localDishes;
                window.Categories = DataGridDishes.ItemsSource.Cast<Dish>()
                    .Select(x => x.Category).Distinct().ToArray();
                bool? res = window.ShowDialog();
                if (res == true) SyncDB();
            }

            if (currentData == DataType.Personal)
            {
                var window = new WindowAddPersonal();
                window.Owner = this;
                bool? res = window.ShowDialog();
                if (res == true) SyncDB();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
            => SyncDB();

        #endregion


        // Методы, вызываемые событиями после нажатия кнопки в контестном меню
        #region Context Menu Events

        private void rowEdit_Click(object sender, RoutedEventArgs e)
        {
            if (currentData == DataType.Personal)
            {
                string tempPersonal = (listBoxPersonal.SelectedItem != null ? listBoxPersonal.SelectedItem : currentItem).ToString();
                var window = new WindowEditPersonal(tempPersonal);
                window.Owner = this;
                bool? res = window.ShowDialog();
                if (res == true) SyncDB();
            }
            else if (currentData == DataType.Dishes)
            {
                Dish tempDish = (Dish)(DataGridDishes.CurrentItem != null ? DataGridDishes.CurrentItem : currentItem);
                var window = new WindowEditDish(tempDish, localDishes);
                window.Categories = DataGridDishes.ItemsSource.Cast<Dish>()
                    .Select(x => x.Category).Distinct().ToArray();
                window.Owner = this;
                window.comboBoxCategory.Text = tempDish.Category;
                bool? res = window.ShowDialog();
                if (res == true) SyncDB();
            }
        }

        private void rowDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentData == DataType.Personal)
            {
                DBConnector.RemoveOficiant(listBoxPersonal.SelectedItem.ToString());
                SyncDB();
            }
            else if (currentData == DataType.Dishes)
            {
                DBConnector.RemoveDishAtName((DataGridDishes.CurrentItem as Dish).Name);
                SyncDB();
            }
        }

        #endregion


        void OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoading && DataGridDishes.CurrentItem != null)
            {
                DBConnector.ChangeDishParameter("isavailable", (DataGridDishes.CurrentItem as Dish).Name, "1");
            }
        }

        void OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoading && DataGridDishes.CurrentItem != null)
            {
                DBConnector.ChangeDishParameter("isavailable", (DataGridDishes.CurrentItem as Dish).Name, "0");
            }
        }

        private void table_SelectionChanged(object sender, RoutedEventArgs e)
        {
            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = true;
            if (currentData == DataType.Personal) currentItem = listBoxPersonal.SelectedItem;
            else if (currentData == DataType.Dishes) currentItem = DataGridDishes.CurrentItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Эта кнопка ничего не делает, спасибо за внимательность!", "Внимание", MessageBoxButton.OK,
                MessageBoxImage.Information, MessageBoxResult.OK);
        }
    }
}
