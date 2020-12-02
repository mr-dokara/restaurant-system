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

                IsLoading = true;
                SynchronizationTask = Task.Factory.StartNew(async () =>
                {
                    bool next = false;
                    do
                    {
                        try
                        {
                            if (Directory.Exists("images"))
                                Directory.Delete("images", true);
                            next = true;
                        }
                        catch
                        {
                            await Task.Delay(2000);
                        }
                    } while (!next);

                    try
                    {
                        if (currentData == DataType.Dishes)
                        {
                            var dbDishes = DBConnector.GetDishes();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
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

        private void Clear_buttonOnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить все записи?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (currentData == DataType.Dishes)
                {
                    DBConnector.ClearDishes();
                    SyncDB();
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

        private void AddNew_ButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (currentData == DataType.Dishes)
            {
                var window = new WindowAddDish();
                window.Owner = this;
                window.Categories = DataGridDishes.ItemsSource.Cast<Dish>()
                    .Select(x => x.Category).Distinct().ToArray();
                window.ShowDialog();
                SyncDB();
            }

            if (currentData == DataType.Personal)
            {
                var window = new WindowAddPersonal();
                window.Owner = this;
                window.ShowDialog();
                SyncDB();
            }
        }

        private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
            => SyncDB();

        #endregion


        // Методы, вызываемые событиями после нажатия кнопок в контестном меню
        #region Context Menu Events

        private void PersonalEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new WindowEditPersonal(listBoxPersonal.SelectedItem.ToString());
            window.Owner = this;
            bool? res = window.ShowDialog();
            if (res == true) SyncDB();
        }

        private void PersonalDelete_OnClick(object sender, RoutedEventArgs e)
        {
            DBConnector.RemoveOficiant(listBoxPersonal.SelectedItem.ToString());
            SyncDB();
        }

        private void DishEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new WindowEditDish(DataGridDishes.CurrentItem as Dish);
            window.Categories = DataGridDishes.ItemsSource.Cast<Dish>()
                .Select(x => x.Category).Distinct().ToArray();
            window.Owner = this;
            window.comboBoxCategory.Text = (DataGridDishes.CurrentItem as Dish).Category;
            bool? res = window.ShowDialog();
            if (res == true) SyncDB();
        }

        private void DishDelete_OnClick(object sender, RoutedEventArgs e)
        {
            DBConnector.RemoveDishAtName((DataGridDishes.CurrentItem as Dish).Name);
            SyncDB();
        }

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

        #endregion
    }
}
