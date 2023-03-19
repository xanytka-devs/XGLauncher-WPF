using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using XGL.Networking.Database;
using XGL.SLS;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Games.xaml
    /// </summary>

    public class XGLApp {
        public string Name { get; private set; }
        public int Status { get; private set; }
        public string Link { get; private set; }
        public bool Custom { get; private set; }
        public string Background { get; set; }
        public string Logo { get; set; }
        public int LoadStatus { get; set; } = 0;
        public XGLApp(string name, int status, string link, bool custom = false) {
            Name = name;
            Status = status;
            Link = link;
            Custom = custom;
        }
        public XGLApp(string name, int status, string link, string background, string logo) {
            Name = name;
            Status = status;
            Link = link;
            Background = background;
            Logo = logo;
        }
    }

    public partial class Games : UserControl {

        readonly List<string> locals = new List<string>();
        bool canEnableLaunchBtn = true;
        bool gbarstatus = true;
        bool reloading = false;
        XGLApp SelectedPoint;
        readonly List<ToggleButton> toggles = new List<ToggleButton>();
        public List<XGLApp> apps = new List<XGLApp>();
        bool oldStyle = false;
        string gBtnStyleName = "GameBtn";

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
                    gGamesBar.Margin = new Thickness(7, 110, 7, 35);
                    addGameBtn.Visibility = Visibility.Visible;
                } else {
                    gGamesBar.Margin = new Thickness(7, 75, 7, 35);
                    addGameBtn.Visibility = Visibility.Collapsed;
                }
                //Update old style.
                oldStyle = RegistrySLS.LoadBool("OldStyle", false);
                if (oldStyle) {
                    LaunchPanelBG.Opacity = 1;
                    gBarV.Opacity = 1;
                    LibLogo.Visibility = Visibility.Hidden;
                    LibBG.Visibility = Visibility.Hidden;
                    gSearchBarTB.Style = (Style)FindResource("OldStyleTextBox");
                    addGameBtn.Style = (Style)FindResource("OldStyleBigButtonGray");
                    gBtnStyleName = "OldStyleGameBtn";
                } else {
                    LaunchPanelBG.Opacity = 0.9;
                    gBarV.Opacity = 0.25;
                    LibLogo.Visibility = Visibility.Collapsed;
                    LibBG.Visibility = Visibility.Visible;
                    gBtnStyleName = "GameBtn";
                }
                //Parse products and check if they are on user's account.
                Database.ParseApps();
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
                if (SelectedPoint != null) OpenPageOf(SelectedPoint);
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
                if (RegistrySLS.LoadBool("GBarAddGame", false)) {
                    gGamesBar.Margin = new Thickness(7, 0, 7, 35);
                    addGameBtn.Visibility = Visibility.Collapsed;
                } else {
                    gGamesBar.Margin = new Thickness(7, 0, 7, 35);
                }
                gBarRow.Width = new GridLength(50);
                gSearchBar.Visibility = Visibility.Collapsed;
                GBarBtn.Content = ">";
                gbarstatus = false;
                RegistrySLS.Save("GBarStatus", false);
            } else {
                if (RegistrySLS.LoadBool("GBarAddGame", false)) {
                    gGamesBar.Margin = new Thickness(7, 110, 7, 35);
                    addGameBtn.Visibility = Visibility.Visible;
                } else {
                    gGamesBar.Margin = new Thickness(7, 75, 7, 35);
                }
                gBarRow.Width = new GridLength(265);
                gSearchBar.Visibility = Visibility.Visible;
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
                    else if (Database.AppsOnAccount[0] == "*") toggles[i].Visibility = Visibility.Visible;
                    else if (Database.AppsOnAccount[i] == "1") toggles[i].Visibility = Visibility.Visible;
                    else if (apps[i].Custom) toggles[i].Visibility = Visibility.Visible;
                }
            } else {
                for (int i = 0; i < toggles.Count; i++) toggles[i].Visibility = Visibility.Visible;
            }
        }
        async void LoadImage(XGLApp app) {
            Image[] img = new Image[] { LibLogo, LibBG };
            for (int i = 0; i < 2; i++) {
                string logoName = Path.Combine(App.CurrentFolder, "cache", app.Logo.Split('{')[0] + ".png");
                string loadURL = app.Logo.Split('{')[1];
                if (i == 1) {
                    logoName = Path.Combine(App.CurrentFolder, "cache", app.Background.Split('{')[0] + ".png");
                    loadURL = app.Background.Split('{')[1];
                }
                if (!File.Exists(logoName))
                    await Utils.DownloadFileAsync(loadURL, logoName);
                BitmapImage logo = new BitmapImage();
                logo.CacheOption = BitmapCacheOption.OnLoad;
                logo.BeginInit();
                logo.UriSource = new Uri(logoName);
                logo.EndInit();
                img[i].Source = logo;
                logo.BaseUri = null;
            }
        }
        void ToggleBtn_Click(object sender, RoutedEventArgs e) {
            launchBtn.Visibility = Visibility.Visible;
            ToggleButton tb = sender as ToggleButton;
            if (LibLogo.Visibility != Visibility.Hidden) {
                BitmapImage logo = new BitmapImage();
                logo.CacheOption = BitmapCacheOption.OnLoad;
                logo.BeginInit();
                logo.UriSource = new Uri("pack://application:,,,/Images/Gradients/RB Gradient Background 045.jpg");
                logo.EndInit();
                LibBG.Source = logo;
                LoadImage(apps[toggles.IndexOf(tb)]);
                LibLogo.Visibility = Visibility.Visible;
            }
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
            string ids = Database.GetAppIDs();
            if (ids != "-") {
                MySqlCommand command = new MySqlCommand($"SELECT * FROM `xgl_products` WHERE `id` IN ({ids})", Database.Connection);
                if (ids == "*") command = new MySqlCommand($"SELECT * FROM `xgl_products`", Database.Connection);
                try {
                    //Fill adapter.
                    adapter.SelectCommand = command;
                    adapter.Fill(table);
                    //Open connection and read everything, what needed.
                    Database.OpenConnection();
                    MySqlDataReader dr = command.ExecuteReader();
                    while (dr.Read()) {
                        apps.Add(new XGLApp(dr.GetString("name"),
                                            dr.GetInt16("availability"),
                                            dr.GetString("latestDownloadLinks"),
                                            dr.GetString("libraryBackground"),
                                            dr.GetString("logo")));
                    }
                    //Close reader and connection.
                    dr.Close();
                    Database.CloseConnection();
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
                    Style = (Style)TryFindResource(gBtnStyleName),
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
