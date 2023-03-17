using XGL;
using XGL.Networking;
using System.Windows;
using System.Windows.Controls.Primitives;
using XGL.SLS;
using System;

namespace XGL.Dev {

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window {

        public static MainWindow Instance { get; private set; }

        public MainWindow() {

            InitializeComponent();
            ApplyLocalization(RegistrySLS.LoadXGLString("Language"));
            Instance = this;
            Reload();

        }

        public void Reload() {
            ProductsP.Reload();
        }

        void MainBtn_Click(object sender, RoutedEventArgs e) {
            switch ((sender as ToggleButton).Name) {
                case "Products":
                    SetAllButtons(Products, ProductsP);
                    break;
                case "Statistics":
                    SetAllButtons(Statistics, StatisticsP);
                    break;
                case "Command":
                    SetAllButtons(Command, CommandP);
                    break;
            }
        }

        void SetAllButtons(ToggleButton nd, UIElement page) {
            Products.IsChecked = false;
            Statistics.IsChecked = false;
            Command.IsChecked = false;
            nd.IsChecked = true;
            ProductsP.Visibility = Visibility.Collapsed;
            StatisticsP.Visibility = Visibility.Collapsed;
            CommandP.Visibility = Visibility.Collapsed;
            page.Visibility = Visibility.Visible;
        }

        public void ApplyLocalization(string localization) {
            //Notify controls.
            //Change inner controls.
            switch (localization) {
                case "ru-RU":
                    Products.Content = "Продукты";
                    Statistics.Content = "Статистика";
                    Command.Content = "Команда";
                    break;
                case "en-US":
                    Products.Content = "Products";
                    Statistics.Content = "Statistics";
                    Command.Content = "Command";
                    break;
                case "es":
                    Products.Content = "Productos";
                    Statistics.Content = "Estadísticas";
                    Command.Content = "Equipo";
                    break;
                case "ru-IM":
                    Products.Content = "Продукты";
                    Statistics.Content = "Статистика";
                    Command.Content = "Команда";
                    break;
            }
        }

    }

}
