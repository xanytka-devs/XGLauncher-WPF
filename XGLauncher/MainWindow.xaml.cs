using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public void CloseAllWindows() {
            for (int i = 0; i < AllWindows.Count; i++) {
                AllWindows[i]?.Close();
            }
            Close();
        }

        public static void Show(object sender) {
            MainWindow m = new MainWindow();
            m.Show();
            (sender as Window).Close();
        }

        public void Reload() {
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
            gamesControl.Clear();
            newsControl.Clear();
            storeControl.Clear();
            gamesControl.Reload();
            ApplyLocalization(RegistrySLS.LoadString("Language", "en-US"));
            LoadImage();
            //Reload parts.
            EnableControls();
        }

        async void LoadImage() {
            if(!App.RunMySQLCommands) return;
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
            storeControl.Clear();
            newsControl.Clear();
            gamesControl.Clear();
            if (sender == store) {
                SetAllButtons(store);
                CollapseAllPages(StorePg);
                storeControl.Reload();
            } else if (sender == news) {
                SetAllButtons(news);
                CollapseAllPages(NewsPg);
                newsControl.Reload();
            } else if (sender == games) {
                SetAllButtons(games);
                CollapseAllPages(GamesPg);
                gamesControl.Reload();
            } else if (sender == community) {
                SetAllButtons(community);
                CollapseAllPages(CommunityPg);
            } else if (sender == profileBtn) {
                SetAllButtons(profileBtn);
                CollapseAllPages(ProfilePg);
                profileControl.Reload();
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
            App.CurrentAccount = new Account("Not Set", "Not Set");
            App.AccountData = null;
            LoginWindow lw = new LoginWindow();
            lw.Show();
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
