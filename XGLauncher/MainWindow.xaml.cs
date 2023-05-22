using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Perception.People;
using XGL.Dialogs;
using XGL.Dialogs.Login;
using XGL.Networking;
using XGL.Pages.LW;
using XGL.SLS;
using Forms = System.Windows.Forms;

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
        bool forceExit = false;

        public void CloseAllWindows() {
            for (int i = 0; i < AllWindows.Count; i++) AllWindows[i]?.Close();
            Close();
        }

        public static void Show(object sender) {
            new MainWindow().Show();
            (sender as Window).Close();
        }

        public void Reload() {
            //Update controls.
            CheckTheme();
            UpdateControls(false);
            //Update public variables.
            myProfile.Text = RegistrySLS.LoadString("Username");
            App.IsPremium = Database.IsPremium(App.CurrentAccount);
            Manager = new VersionManager();
            //Update visibility of dev elements.
            community.Visibility = Visibility.Collapsed;
            plg_btn.Visibility = Visibility.Collapsed;
            if (RegistrySLS.LoadBool("Community", false)) {
                community.Visibility = Visibility.Visible;
                MinWidth = 1040;
            }
            if (RegistrySLS.LoadBool("Plugins", false) &&
                RegistrySLS.LoadBool("PluginsButton", false)) plg_btn.Visibility = Visibility.Visible;
            storeControl.Clear();
            gamesControl.Clear();
            newsControl.Clear();
            if (curP == 1) storeControl.Reload();
            if(curP == 2) gamesControl.Reload();
            if(curP == 3) newsControl.Reload();
            ApplyLocalization();
            LoadImage();
            //Check for unsupportion.
            CheckVersion();
            //Reload parts.
            UpdateControls(true);
            //Tray icon.
            App.TrayIcon?.Dispose();
            CreateTrayIcon();
        }

        void GetPosSize() {

            if (!RegistrySLS.LoadBool("SavePosSize", true)) return;
            string saved = RegistrySLS.LoadString("PosSize", "INS");
            if(saved == "INS") {
                SetPosSize();
                return;
            }
            string[] saveData = saved.Split(';');
            if (saveData[0] == "max") {
                WindowState = WindowState.Maximized;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }
            Height = double.Parse(saveData[2]);
            Width = double.Parse(saveData[3]);
            Point pos = new Point(double.Parse(saveData[0]), double.Parse(saveData[1]));
            Left = pos.X;
            Top = pos.Y;

        }

        void SetPosSize() {
            if (!RegistrySLS.LoadBool("SavePosSize", true)) return;
            Point pos = new Point(Left, Top);
            RegistrySLS.Save("PosSize", $"{pos.X};{pos.Y};{Height};{Width}");
            if(WindowState == WindowState.Maximized) RegistrySLS.Save("PosSize", $"max");
        }

        async void CheckVersion() {
            while (!LocalizationManager.I.LocalLoaded()) await Task.Delay(25);
            string curSup = Database.GetVersionSupport();
            if(bool.Parse(curSup.Split(';')[0])) {
                try {
                    TimeSpan days = new TimeSpan(DateTime.Parse(curSup.Split(';')[1]).Subtract(
                        new TimeSpan(DateTime.Now.Ticks)).Date.Ticks);
                    supUntil.Text = LocalizationManager.I.dictionary["mw.warn.unsup"]
                        .Replace("{0}", days.Days.ToString());
                }
                catch {
                    supUntil.Text = LocalizationManager.I.dictionary["mw.warn.nosup"];
                }
            } else HideUnsupportWarn(null, null);
        }

        public void ReloadTheme() {

            Close();
            MainWindow m = new MainWindow();
            m.Show();
            m.gamesControl.Reload();

        }

        void CheckTheme() {
            //TODO: Implement. Please.
            //string theme = RegistrySLS.LoadString("Theme", "System");
            //var uri = new Uri("Themes\\" + "Dark" + ".xaml", UriKind.Relative);
            //if (theme != "System")
            //    uri = new Uri("Themes\\" + theme + ".xaml", UriKind.Relative);
            //ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            //Application.Current.Resources.Clear();
            //Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        async void LoadImage() {
            if(!App.RunMySQLCommands || App.CurrentAccount == null) return;
            string URL;
            if (App.IsFirstRun) URL = "default{https://drive.google.com/uc?export=download&id=1hKSUYQgTaJIp8V-coY8Y8Bmod0eIupzy";
            else URL = Database.GetValue(App.CurrentAccount, "icon").ToString();
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(URL.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage {
                CacheOption = BitmapCacheOption.OnLoad
            };
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            profileIcon.Source = logo;
        }

        public MainWindow() {

            InitializeComponent();
            Instance = this;
            Loaded += delegate { GetPosSize(); };
            if(!RegistrySLS.LoadBool("SavePosSize", true))
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ApplyLocalization();
            Reload();
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => {
                if(RegistrySLS.LoadBool("SavePosSize", true)) SetPosSize();
                if(!RegistrySLS.LoadBool("HideInTray", true) || forceExit) {
                    App.TrayIcon?.Dispose();
                    Application.Current.Shutdown();
                }
                e.Cancel = true;
                Hide();
            };

        }

        async void CreateTrayIcon() {
            if (!RegistrySLS.LoadBool("HideInTray", true)) return;
            App.TrayIcon = new Forms.NotifyIcon {
                Icon = new System.Drawing.Icon("xgl.ico"),
                Text = "XGLauncher",
                Visible = true,
                ContextMenuStrip = new Forms.ContextMenuStrip()
            };
            LocalizationManager l = LocalizationManager.I;
            while (!l.LocalLoaded()) await Task.Delay(25);
            App.TrayIcon.ContextMenuStrip.Items.Add(l.dictionary["mw.store"], null,
                (object sender, EventArgs e) => {
                    Instance.Show();
                    Instance.Activate();
                    CheckTabs(store);
                });
            App.TrayIcon.ContextMenuStrip.Items.Add(l.dictionary["mw.news"], null,
                (object sender, EventArgs e) => {
                    Instance.Show();
                    Instance.Activate();
                    CheckTabs(news);
                });
            App.TrayIcon.ContextMenuStrip.Items.Add(l.dictionary["mw.games"], null,
                (object sender, EventArgs e) => {
                    Instance.Show();
                    Instance.Activate();
                    CheckTabs(games);
                });
            App.TrayIcon.ContextMenuStrip.Items.Add(l.dictionary["mw.oti"], null,
                (object sender, EventArgs e) => {
                    Instance.Show();
                    Instance.Activate();
                });
            App.TrayIcon.ContextMenuStrip.Items.Add(l.dictionary["gn.exit"], null,
                (object sender, EventArgs e) => {
                    forceExit = true;
                    Instance.Close();
                    Application.Current.Shutdown();
                });
        }

        void TrayIcon_LMB(object sender, EventArgs e) {
            Instance.Show();
        }

        public void MainBtn_Click(object sender, RoutedEventArgs e) {
            if (curP != 1)
                storeControl.Clear();
            if (curP != 3)
                newsControl.Clear();
            CheckTabs(sender);
            GC.Collect();
            App.IsFirstRun = false;
            (sender as ToggleButton).IsChecked = true;
        }

        internal void CheckTabs(object sender) {
            if(RegistrySLS.LoadBool("DClickToReloadTab", false))
                curP = 0;
            if(sender != games) gamesControl.tabClicks = 0;
            if(sender != store) storeControl.tabClicks = 0;
            if(sender != news) newsControl.tabClicks = 0;
            if (sender != community) communityControl.tabClicks = 0;
            if (sender == store & curP != 1) {
                SetAllButtons(store);
                CollapseAllPages(StorePg);
                storeControl.Reload();
                storeControl.tabClicks++;
                curP = 1;
            } else if (sender == games & curP != 2) {
                SetAllButtons(games);
                CollapseAllPages(GamesPg);
                gamesControl.tabClicks++;
                curP = 2;
            } else if (sender == news & curP != 3) {
                SetAllButtons(news);
                CollapseAllPages(NewsPg);
                newsControl.Reload();
                newsControl.tabClicks++;
                curP = 3;
            } else if (sender == community & curP != 4) {
                SetAllButtons(community);
                CollapseAllPages(CommunityPg);
                communityControl.tabClicks++;
                curP = 4;
            } else if (sender == myProfileBtn & curP != 5) {
                SetAllButtons(myProfileBtn);
                CollapseAllPages(MyProfilePg);
                myProfileControl.Reload();
                curP = 5;
            }
        }

        public void CollapseAllPages(Border nc) {
            StorePg.Visibility = Visibility.Collapsed;
            NewsPg.Visibility = Visibility.Collapsed;
            GamesPg.Visibility = Visibility.Collapsed;
            CommunityPg.Visibility = Visibility.Collapsed;
            MyProfilePg.Visibility = Visibility.Collapsed;
            PublisherPg.Visibility = Visibility.Collapsed;
            DownloadsPg.Visibility = Visibility.Collapsed;
            ProfilePg.Visibility = Visibility.Collapsed;
            nc.Visibility = Visibility.Visible;
        }

        void SetAllButtons(ToggleButton nc) {
            store.IsChecked = false;
            news.IsChecked = false;
            games.IsChecked = false;
            community.IsChecked = false;
            myProfileBtn.IsChecked = false;
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

        public void OpenProfileOf(long ID) {
            CollapseAllPages(ProfilePg);
            ProfileControl.ID = ID;
        }

        public void UpdateControls(bool s) {
            gamesControl.UpdateControls(s);
            store.IsEnabled = s;
            news.IsEnabled = s;
            games.IsEnabled = s;
            community.IsEnabled = s;
            myProfile.IsEnabled = s;
        }

        public async void ApplyLocalization() {
            LocalizationManager l = LocalizationManager.I;
            while (!l.LocalLoaded()) await Task.Delay(25);
            //Notify controls.
            storeControl.ApplyLocalization();
            newsControl.ApplyLocalization();
            gamesControl.ApplyLocalization();
            communityControl.ApplyLocalization();
            myProfileControl.ApplyLocalization();
            //Change inner controls.
            store.Content = l.dictionary["mw.store"];
            news.Content = l.dictionary["mw.news"];
            games.Content = l.dictionary["mw.games"];
            community.Content = l.dictionary["mw.community"];
        }

        public void ReturnRequest(string c/*, WebResponse response*/) {
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

        void HideUnsupportWarn(object sender, RoutedEventArgs e) {
            UnsupportWarn.Visibility = Visibility.Collapsed;
            mainGrid.RowDefinitions.Remove(UnSupBar);
        }

    }

}
