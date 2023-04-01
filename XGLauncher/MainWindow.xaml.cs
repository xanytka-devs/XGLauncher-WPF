using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XGL.Dialogs;
using XGL.Dialogs.Login;
using XGL.Networking;
using XGL.Networking.Database;
using XGL.SLS;

namespace XGL {

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window {

        public OptionsWindow settingsW;
        public static MainWindow Instance { get; private set; }
        public static VersionManager Manager { get; private set; }
        public static bool OODDialofResult { get; set; }
        public List<Window> AllWindows = new List<Window>();
        public int curP = 2;

        public void CloseAllWindows() {
            for (int i = 0; i < AllWindows.Count; i++) AllWindows[i]?.Close();
            Close();
        }

        public static void Show(object sender) {
            new MainWindow().Show();
            (sender as Window).Close();
        }

        public void Reload() {
            CheckTheme();
            DisableControls();
            //Update public variables.
            profile.Text = RegistrySLS.LoadString("Username");
            Manager = new VersionManager();
            //Update visibility of dev elements.
            community.Visibility = Visibility.Collapsed;
            plg_btn.Visibility = Visibility.Collapsed;
            if (RegistrySLS.LoadBool("Community", false)) community.Visibility = Visibility.Visible;
            if (RegistrySLS.LoadBool("Plugins", false) &&
                RegistrySLS.LoadBool("PluginsButton", false)) plg_btn.Visibility = Visibility.Visible;
            storeControl.Clear();
            gamesControl.Clear();
            newsControl.Clear();
            if (curP == 1) storeControl.Reload();
            if(curP == 2) gamesControl.Reload();
            if(curP == 3) newsControl.Reload();
            ApplyLocalization(RegistrySLS.LoadString("Language", "en-US"));
            LoadImage();
            //Reload parts.
            EnableControls();
        }

        public void ReloadTheme() {

            Close();
            MainWindow m = new MainWindow();
            m.Show();
            m.gamesControl.Reload();

        }

        void CheckTheme() {
            string theme = RegistrySLS.LoadString("Theme", "System");
            var uri = new Uri("Themes\\" + "Dark" + ".xaml", UriKind.Relative);
            if (theme != "System")
                uri = new Uri("Themes\\" + theme + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            switch (theme) {
                case "Light":
                    TopBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#818181"));
                    break;
                case "Ohio":
                    TopBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10dd55"));
                    break;
            }
        }

        async void LoadImage() {
            if(!App.RunMySQLCommands || App.CurrentAccount == null) return;
            string URL = string.Empty;
            if (App.IsFirstRun) URL = "default{https://drive.google.com/uc?export=download&id=1hKSUYQgTaJIp8V-coY8Y8Bmod0eIupzy";
            else URL = Database.GetValue(App.CurrentAccount, "icon").ToString();
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
                await Utils.DownloadFileAsync(URL.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.CacheOption = BitmapCacheOption.OnLoad;
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            profileIcon.Source = logo;
        }

        public MainWindow() {

            InitializeComponent();
            Instance = this;
            ApplyLocalization(RegistrySLS.LoadString("Language", "en-US"));
            Reload();

        }

        public void MainBtn_Click(object sender, RoutedEventArgs e) {
            if (curP != 1)
                storeControl.Clear();
            if (curP != 3)
                newsControl.Clear();
            if (curP != 2)
                gamesControl.Clear();
            if (sender == store & curP != 1) {
                SetAllButtons(store);
                CollapseAllPages(StorePg);
                storeControl.Reload();
                curP = 1;
            } else if (sender == games & curP != 2) {
                SetAllButtons(games);
                CollapseAllPages(GamesPg);
                gamesControl.Reload();
                curP = 2;
            } else if (sender == news & curP != 3) {
                SetAllButtons(news);
                CollapseAllPages(NewsPg);
                newsControl.Reload();
                curP = 3;
            } else if (sender == community & curP != 4) {
                SetAllButtons(community);
                CollapseAllPages(CommunityPg);
                curP = 4;
            } else if (sender == profileBtn & curP != 5) {
                SetAllButtons(profileBtn);
                CollapseAllPages(ProfilePg);
                profileControl.Reload();
                curP = 5;
            }
            GC.Collect();
            App.IsFirstRun = false;
        }

        void CollapseAllPages(Border nc) {
            StorePg.Visibility = Visibility.Collapsed;
            NewsPg.Visibility = Visibility.Collapsed;
            GamesPg.Visibility = Visibility.Collapsed;
            CommunityPg.Visibility = Visibility.Collapsed;
            ProfilePg.Visibility = Visibility.Collapsed;
            nc.Visibility = Visibility.Visible;
        }

        void SetAllButtons(ToggleButton nc) {
            store.IsChecked = false;
            news.IsChecked = false;
            games.IsChecked = false;
            community.IsChecked = false;
            profileBtn.IsChecked = false;
            nc.IsChecked = true;
        }

        void OpenSettings(object sender, RoutedEventArgs e) {
            if (settingsW == null)
                settingsW = new OptionsWindow();
            settingsW.Show();
            settingsW.Focus();
        }

        void OpenPlugins(object sender, RoutedEventArgs e)  {
            PluginWindow pluginWindow = new PluginWindow();
            pluginWindow.Show();
        }

        //SPECIAL FUNCTIONS//
        public void SignOut() {
            Database.SetValue(Database.DBDataType.DT_ACTIVITY, 0);
            Database.SetValue(Database.DBDataType.DT_LASTONLINE, DateTime.Now);
            RegistrySLS.Save("Description", "INS");
            RegistrySLS.Save("LastID", "INS");
            RegistrySLS.Save("LoginData", "INS");
            App.CurrentAccount = new Account("INS", "INS");
            App.AccountData = null;
            Process.Start(Path.Combine(App.CurrentFolder, "XGLauncher.exe"));
            Close();
        }

        public void DisableControls() {
            gamesControl.DisableControls();
            store.IsEnabled = false;
            news.IsEnabled = false;
            games.IsEnabled = false;
            community.IsEnabled = false;
            profile.IsEnabled = false;
        }

        public void EnableControls() {
            gamesControl.EnableControls();
            store.IsEnabled = true;
            news.IsEnabled = true;
            games.IsEnabled = true;
            community.IsEnabled = true;
            profile.IsEnabled = true;
        }

        public void ApplyLocalization(string localization) {
            //Notify controls.
            storeControl.ApplyLocalization(localization);
            newsControl.ApplyLocalization(localization);
            gamesControl.ApplyLocalization(localization);
            communityControl.ApplyLocalization(localization);
            profileControl.ApplyLocalization(localization);
            //Change inner controls.
            switch (localization) {
                case "ru-RU":
                    store.Content = "Магазин";
                    news.Content = "Новости";
                    games.Content = "Библиотека";
                    community.Content = "Сообщество";
                    break;
                case "en-US":
                    store.Content = "Store";
                    news.Content = "News";
                    games.Content = "Library";
                    community.Content = "Community";
                    break;
                case "es":
                    store.Content = "Almacenar";
                    news.Content = "Noticia";
                    games.Content = "Biblioteca";
                    community.Content = "Comunidad";
                    break;
                case "ru-IM":
                    store.Content = "Магазинъ";
                    news.Content = "Новости";
                    games.Content = "Библіотека";
                    community.Content = "Сообщество";
                    break;
            }
        }

        public void ReturnRequest(string c, WebResponse response) {
            switch (c) {
                case "google":
                    break;
                case "yt":
                    break;
                case "vk":
                    break;
                case "tg":
                    break;
                case "ds":
                    break;
            }
        }

    }

}
