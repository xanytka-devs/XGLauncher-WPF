using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using XGL.Networking.Database;
using XGL.SLS;
using static System.Net.Mime.MediaTypeNames;
using static XGL.Networking.Database.Database;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Games.xaml
    /// </summary>

    public class XGLApp {
        public string Name { get; private set; }
        public int Status { get; private set; }
        public string Link { get; private set; }
        public bool Custom { get; private set; }
        public int LoadStatus { get; set; } = 0;
        public XGLApp(string name, int status, string link, bool custom = false) {
            Name = name;
            Status = status;
            Link = link;
            Custom = custom;
        }
    }

    public partial class Games : UserControl {

        List<string> locals = new List<string>();
        bool canEnableLaunchBtn = true;
        bool gbarstatus = true;
        bool reloading = false;
        XGLApp SelectedPoint;
        readonly List<ToggleButton> toggles = new List<ToggleButton>();
        public List<XGLApp> apps = new List<XGLApp>();

        public Games() {
            InitializeComponent();
        }

        public void Reload() {
            if(!reloading) {
                reloading = true;
                DisableControls();
                //Change bar status.
                if (!RegistrySLS.LoadBool("GBarStatus", true)) GBarChangeStatus(true);
                else GBarChangeStatus(false);
                //Update visibility of dev elements.
                if (RegistrySLS.LoadBool("GBarAddGame", false)) {
                    gGamesBar.Margin = new Thickness(10, 110, 5, 35);
                    addGameBtn.Visibility = Visibility.Visible;
                } else {
                    gGamesBar.Margin = new Thickness(10, 75, 5, 35);
                    addGameBtn.Visibility = Visibility.Collapsed;
                }
                //Parse products and check if they are on user's account.
                ParseApps();
                //Check database for products.
                CheckDatabase();
                //Check apps for existance/updates.
                CheckApps();
                //Read custom apps.
                ReadFromGamesFile();
                //Generate buttons.
                GenerateButtons();
                //Open the page of app.
                EnableControls();
                if (SelectedPoint == null) SelectedPoint = apps.FirstOrDefault();
                OpenPageOf(SelectedPoint);
                reloading = false;
            }
        }
        public void Clear() {
            gBarSP.Children.Clear();
            toggles.Clear();
            apps.Clear();
        }

        void GBarBtn_Click(object sender, RoutedEventArgs e) { 
            if (gbarstatus) GBarChangeStatus(true);
            else GBarChangeStatus(false);
        }
        void GBarChangeStatus(bool value) {
            if (value) {
                gBarRow.Width = new GridLength(50);
                gSearchBar.Visibility = Visibility.Collapsed;
                gGamesBar.Margin = new Thickness(0, 0, 0, 35);
                GBarBtn.Content = ">";
                gbarstatus = false;
                RegistrySLS.Save("GBarStatus", false);
            } else {
                gBarRow.Width = new GridLength(265);
                gSearchBar.Visibility = Visibility.Visible;
                gGamesBar.Margin = new Thickness(0, 93, 0, 35);
                GBarBtn.Content = "<";
                gbarstatus = true;
                RegistrySLS.Save("GBarStatus", true);
            }
        }
        void GSearchBarTB_TextChanged(object sender, TextChangedEventArgs e) {
            if (!string.IsNullOrEmpty(gSearchBarTB.Text)) {
                for (int i = 0; i < toggles.Count; i++) {
                    if (!toggles[i].Content.ToString().ToLower().Contains(gSearchBarTB.Text.ToLower()))
                        toggles[i].Visibility = Visibility.Collapsed;
                    else if (AppsOnAccount[0] == "*") toggles[i].Visibility = Visibility.Visible;
                    else if (AppsOnAccount[i] == "1") toggles[i].Visibility = Visibility.Visible;
                    else if (apps[i].Custom) toggles[i].Visibility = Visibility.Visible;
                }
            } else {
                for (int i = 0; i < toggles.Count; i++) toggles[i].Visibility = Visibility.Visible;
            }
        }
        void ToggleBtn_Click(object sender, RoutedEventArgs e) {
            launchBtn.Visibility = Visibility.Visible;
            ToggleButton tb = sender as ToggleButton;
            for (int j = 0; j < toggles.Count; j++) toggles[j].IsChecked = false;
            tb.IsChecked = true;
            SelectedPoint = tb.DataContext as XGLApp;
            if (canEnableLaunchBtn && apps.Count > 0) {
                if (apps[toggles.IndexOf(tb)].Status.ToString() == "0") launchBtn.IsEnabled = false;
                else launchBtn.IsEnabled = true;
                if (apps[toggles.IndexOf(tb)].LoadStatus == 0) launchBtn.Content = locals[0];
                else launchBtn.Content = locals[1];
            }
        }
        void CheckDatabase() {
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            string ids = GetAppIDs();
            if (ids != "-") {
                MySqlCommand command = new MySqlCommand($"SELECT * FROM `products` WHERE `id` IN ({ids})", Connection);
                if (ids == "*") command = new MySqlCommand($"SELECT * FROM `products`", Connection);
                try {
                    //Fill adapter.
                    adapter.SelectCommand = command;
                    adapter.Fill(table);
                    //Open connection and read everything, what needed.
                    OpenConnection();
                    MySqlDataReader dr = command.ExecuteReader();
                    while (dr.Read()) {
                        apps.Add(new XGLApp(dr.GetString("name"), dr.GetInt16("availability"), dr.GetString("latestDownloadLinks")));
                    }
                    //Close reader and connection.
                    dr.Close();
                    CloseConnection();
                }
                catch (Exception ex) {
                    //TODO: Implement custom dialog system.
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        void GenerateButtons() {
            for (int i = 0; i < apps.Count; i++) {
                Regex sWhitespace = new Regex(@"\s+");
                ToggleButton btn = new ToggleButton {
                    Style = TryFindResource("GameBtn") as Style,
                    Height = 40,
                    Content = GetGoodName(apps[i]),
                    Name = sWhitespace.Replace((apps[i].Name.ToLower() + "tb").Replace("'", string.Empty).Replace(".", string.Empty), string.Empty),
                    DataContext = apps[i]
                };
                btn.Click += ToggleBtn_Click;
                gBarSP.Children.Add(btn);
                toggles.Add(btn);
            }
        }
        void CheckApps() {
            List<string> dirs = new List<string>(Directory.EnumerateDirectories(App.AppsFolder));
            for (int i = 0; i < dirs.Count; i++) {
                for (int j = 0; j < apps.Count; j++) {
                    if (dirs[i].Split('\\')[dirs[i].Split('\\').Length - 1] == apps[j].Name) apps[j].LoadStatus = 1;
                }
            }
        }
        string GetGoodName(XGLApp app) {
            if (app.Custom) return app.Name.Split('.')[0];
            else return app.Name;
        }
        void LaunchBtn_Click(object sender, RoutedEventArgs e) {
            if (SelectedPoint.Custom) {
                MainWindow.Manager.Launch(SelectedPoint, "zip", SelectedPoint.Link);
                return;
            }
            MainWindow.Manager.Launch(SelectedPoint);
        }
        void UpdateBtn_Click(object sender, RoutedEventArgs e) {



        }
        void AddCustomGame(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog() {
                InitialDirectory = App.AppsFolder,
                Filter = "Application (*.exe)|*.exe|All files (*.*)|*.*"
            };
            if(fileDialog.ShowDialog() == true) {
                if (!string.IsNullOrEmpty(fileDialog.FileName)) WriteToGamesFile(fileDialog.FileName);
                else
                    //TODO: Implement custom dialog system.
                    MessageBox.Show("Path empty.", "XGLauncher");
            }
            Reload();
        }
        void WriteToGamesFile(string path) {
            string appsFile = Path.Combine(App.AppsFolder, "Applications");
            if (File.Exists(appsFile)) {
                string text = File.ReadAllText(appsFile);
                text += $"{path.Split('\\')[path.Split('\\').Length - 1]}^{path}\n";
                File.WriteAllText(appsFile, text);
            } else
                File.WriteAllText(appsFile, $"{path.Split('\\')[path.Split('\\').Length - 1]}^{path}\n");
        }
        void ReadFromGamesFile() {
            string appsFile = Path.Combine(App.AppsFolder, "Applications");
            if (File.Exists(appsFile)) {
                string[] applications = File.ReadAllText(appsFile).Split('\n');
                for (int i = 0; i < applications.Length; i++) {
                    if(!string.IsNullOrEmpty(applications[i]))
                        apps.Add(new XGLApp(applications[i].Split('^')[0], 1, applications[i].Split('^')[1], true));
                }
            } else {
                File.Create(appsFile);
                SelectedPoint = apps.FirstOrDefault();
            }
        }
        //SPECIAL FUNCTIONS//
        public void OpenPageOf(XGLApp productName) {
            if(!reloading)
                MainWindow.Instance.MainBtn_Click(MainWindow.Instance.games, null);
            SelectedPoint = productName;
            Regex sWhitespace = new Regex(@"\s+");
            ToggleButton tb = new ToggleButton();
            for (int i = 0; i < toggles.Count; i++) {
                if (toggles[i].Name == sWhitespace.Replace(SelectedPoint.Name.ToLower().Replace("'", string.Empty).Replace(".", string.Empty) + "tb", string.Empty))
                    tb = toggles[i];
            }
            ToggleBtn_Click(tb, null);
        }
        public void DisableControls() { launchBtn.IsEnabled = false; canEnableLaunchBtn = false; }
        public void EnableControls() { launchBtn.IsEnabled = true; canEnableLaunchBtn = true; }
        public void ApplyLocalization(string localization) {
            switch (localization) {
                case "ru-RU":
                    searchBarT.Text = "Поиск";
                    launchBtn.Content = "Запуск";
                    locals.Add("Скачать");
                    locals.Add("Запуск");
                    break;
                case "en-US":
                    searchBarT.Text = "Search";
                    launchBtn.Content = "Launch";
                    locals.Add("Download");
                    locals.Add("Launch");
                    break;
                case "es":
                    searchBarT.Text = "Búsqueda";
                    launchBtn.Content = "Lanzar";
                    locals.Add("Descargar");
                    locals.Add("Lanzar");
                    break;
                case "ru-IM":
                    searchBarT.Text = "Поискъ";
                    launchBtn.Content = "Запускъ";
                    locals.Add("Скачать");
                    locals.Add("Запускъ");
                    break;
            }
        }

    }
}
