using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XGL.Networking;
using XGL.SLS;
using static XGL.Networking.Database;

namespace XGL.Dev.Pages {

    /// <summary>
    /// Логика взаимодействия для Products.xaml
    /// </summary>
    
    public partial class Products : UserControl {

        List<string> apps = new List<string>();
        List<ToggleButton> toggles = new List<ToggleButton>();
        string SelectedPoint;

        public Products() {
            InitializeComponent();
        }

        void AddApp(object sender, RoutedEventArgs e) { 
            CreateProductView.Visibility = Visibility.Visible; 
            ProductView.Visibility = Visibility.Collapsed;
            for (int i = 0; i < toggles.Count; i++) toggles[i].IsChecked = false;
        }

        void GSearchBarTB_TextChanged(object sender, TextChangedEventArgs e) {
            if (!string.IsNullOrEmpty(appSearchBarTB.Text)) {
                for (int i = 0; i < toggles.Count; i++) {
                    if (!toggles[i].Content.ToString().ToLower().Contains(appSearchBarTB.Text.ToLower()))
                        toggles[i].Visibility = Visibility.Collapsed;
                }
            } else {
                for (int i = 0; i < toggles.Count; i++) toggles[i].Visibility = Visibility.Visible;
            }
        }

        public void Reload() {
            appBar.Children.Clear();
            toggles.Clear();
            apps.Clear();
            //Parse database.
            MySqlCommand command = new MySqlCommand($"SELECT `name` FROM `products` WHERE `publisherID` = @pid", Connection);
            //Add parameters
            command.Parameters.Add("@pid", MySqlDbType.VarChar).Value = RegistrySLS.LoadString("LastID");
            //Check if user exists or not.
            try {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) {
                    apps.Add(dr.GetString("name"));
                }
                dr.Close();
                CloseConnection();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
            }
            //Generate buttons.
            GenerateButtons();
        }

        void GenerateButtons() {
            for (int i = 1; i < apps.Count + 1; i++) {
                Regex sWhitespace = new Regex(@"\s+");
                ToggleButton btn = new ToggleButton {
                    Style = TryFindResource("AppBtn") as Style,
                    Height = 40,
                    Content = apps[i - 1],
                    Name = sWhitespace.Replace((apps[i - 1].ToLower() + "tb").Replace("'", string.Empty), string.Empty)
                };
                btn.Click += (object sender, RoutedEventArgs e) => {
                    ToggleButton tb = sender as ToggleButton;
                    for (int j = 0; j < toggles.Count; j++) toggles[j].IsChecked = false;
                    tb.IsChecked = true;
                    SelectedPoint = tb.Content.ToString();
                    CreateProductView.Visibility = Visibility.Collapsed;
                    ProductView.Visibility = Visibility.Visible;
                };
                appBar.Children.Add(btn);
                toggles.Add(btn);
            }
        }
    }
}
