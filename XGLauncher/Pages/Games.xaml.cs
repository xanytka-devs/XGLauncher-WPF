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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XGL.Networking;
using XGL.SLS;
using Forms = System.Windows.Forms;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Games.xaml
    /// </summary>

    public class XGLApp : IComparable {
        public long ID { get; private set; }
        public string Name { get; private set; }
        public int Status { get; private set; } = 1;
        public string Path { get; private set; }
        public bool Custom { get; private set; }
        public string Background { get; set; }
        public string Logo { get; set; }
        public string MiniLogo { get; set; }
        public bool LoadStatus { get; set; } = false;
        public bool NeedsUpdate { get; set; } = false;
        public string Version { get; set; } = "1.0";
        public XGLApp(string name, int status, string path, bool custom = false) {
            Name = name;
            Status = status;
            Path = path;
            Custom = custom;
        }
        public XGLApp(long id, string name, int status, string path, string background, string logo, string miniLogo) {
            ID = id;
            Name = name;
            Status = status;
            Path = path;
            Background = background;
            Logo = logo;
            Version = path.Split('{')[0];
            MiniLogo = miniLogo;
        }
        public XGLApp(long id, string name, string path, bool custom = false, string version = "1.0") {
            ID = id;
            Name = name;
            Path = path;
            Custom = custom;
            Version = version;
        }

        public int CompareTo(object obj) {

            return Name.CompareTo((obj as XGLApp).Name);

        }
    }

    public partial class Games : UserControl {

        readonly List<string> locals = new List<string>();
        bool canEnableLaunchBtn = true;
        bool gbarstatus = true;
        bool reloading = false;
        XGLApp SelectedPoint;
        readonly List<ToggleButton> toggles = new List<ToggleButton>();
        readonly List<Image> miniLogos = new List<Image>();
        readonly List<TextBlock> texts = new List<TextBlock>();
        public List<XGLApp> apps = new List<XGLApp>();
        List<XGLApp> customApps = new List<XGLApp>();
        bool oldStyle = false;
        string gBtnStyleName = "GameBtn";
        internal int tabClicks = 2;

        public Games() {
            InitializeComponent();
        }

        public void Reload() {
            if (RegistrySLS.LoadBool("DClickToReloadTab", false)) {
                if (tabClicks != 2) return;
                Clear();
                tabClicks = 0;
            }
            if (!reloading) {
                reloading = true;
                UpdateControls(false);
                //Check styles.
                CheckStyling();
                //Check apps file.
                ReadFromGamesFile();
                //Parse products and check if they are on user's account.
                Database.ParseApps();
                //Check database for products.
                CheckDatabase();
                //Generate buttons.
                GenerateButtons();
                //Open the page of app.
                UpdateControls(true);
                GBarChangeStatus(!RegistrySLS.LoadBool("GBarStatus", false));
                if (SelectedPoint != null) OpenPageOf(SelectedPoint);
                reloading = false;
            }
        }

        public void Clear() {
            if (RegistrySLS.LoadBool("DClickToReloadTab", false))
                if (tabClicks != 2) return;
            gBarSP.Children.Clear();
            toggles.Clear();
            apps.Clear();
            customApps.Clear();
        }

        void GBarBtn_Click(object sender, RoutedEventArgs e) {
            if (gbarstatus) GBarChangeStatus(true);
            else GBarChangeStatus(false);
        }
        void GBarChangeStatus(bool value) {
            if (value) {
                if (RegistrySLS.LoadBool("GBarAddGame", false))
                    addGameBtn.Visibility = Visibility.Collapsed;
                gBarRow.Width = new GridLength(50);
                gSearchBar.Visibility = Visibility.Collapsed;
                GBarBtn.Content = ">";
                gbarstatus = false;
                RegistrySLS.Save("GBarStatus", false);
                if (oldStyle) {
                    for (int i = 0; i < miniLogos.Count; i++)
                        texts[i].Visibility = Visibility.Collapsed;
                    return;
                }
                for (int i = 0; i < miniLogos.Count; i++) {
                    miniLogos[i].Visibility = Visibility.Visible;
                    texts[i].Visibility = Visibility.Collapsed;
                }
            } else {
                if (RegistrySLS.LoadBool("GBarAddGame", false))
                    addGameBtn.Visibility = Visibility.Visible;
                gBarRow.Width = new GridLength(265);
                gSearchBar.Visibility = Visibility.Visible;
                GBarBtn.Content = "<";
                gbarstatus = true;
                RegistrySLS.Save("GBarStatus", true);
                if (oldStyle) {
                    for (int i = 0; i < miniLogos.Count; i++)
                        texts[i].Visibility = Visibility.Visible;
                    return;
                }
                for (int i = 0; i < miniLogos.Count; i++) {
                    miniLogos[i].Visibility = Visibility.Collapsed;
                    texts[i].Visibility = Visibility.Visible;
                }
            }
        }
        void GSearchBarTB_TextChanged(object sender, TextChangedEventArgs e) {
            if (!string.IsNullOrEmpty(gSearchBarTB.Text)) {
                for (int i = 0; i < toggles.Count; i++) {
                    if (!texts[i].Text.ToString().ToLower().Contains(gSearchBarTB.Text.ToLower()))
                        toggles[i].Visibility = Visibility.Collapsed;
                    else if (Database.AppsOnAccount[0] == "*") toggles[i].Visibility = Visibility.Visible;
                    else if (Database.AppsOnAccount[i] == "1") toggles[i].Visibility = Visibility.Visible;
                    else if (apps[i].Custom) toggles[i].Visibility = Visibility.Visible;
                }
                return;
            }
            for (int i = 0; i < toggles.Count; i++) toggles[i].Visibility = Visibility.Visible;
        }
        async void LoadImage(XGLApp app) {
            Image[] img = new Image[] { LibLogo, LibBG };
            for (int i = 0; i < 2; i++) {
                if (app.Logo == null) return;
                string logoName = Path.Combine(App.CurrentFolder, "cache", app.Logo.Split('{')[0] + ".png");
                string loadURL = app.Logo.Split('{')[1];
                if (i == 1) {
                    logoName = Path.Combine(App.CurrentFolder, "cache", app.Background.Split('{')[0] + ".png");
                    loadURL = app.Background.Split('{')[1];
                }
                if (i == 2) {
                    img[3] = FindName(new Regex(@"\s+").Replace(SelectedPoint.Name, string.Empty) + "_MiniLibLogo") as Image;
                    logoName = Path.Combine(App.CurrentFolder, "cache", app.MiniLogo.Split('{')[0] + ".png");
                    loadURL = app.MiniLogo.Split('{')[1];
                }
                if (!File.Exists(logoName))
                    await Utils.I.DownloadFileAsync(loadURL, logoName);
                BitmapImage logo = new BitmapImage {
                    CacheOption = BitmapCacheOption.OnLoad
                };
                logo.BeginInit();
                logo.UriSource = new Uri(logoName);
                logo.EndInit();
                img[i].Source = logo;
                logo.BaseUri = null;
            }
        }
        async void LoadMiniLogo(XGLApp app, Image caller) {
            if (app.Custom) return;
            string logoName = Path.Combine(App.CurrentFolder, "cache", app.MiniLogo.Split('{')[0] + ".png");
            string loadURL = app.MiniLogo.Split('{')[1];
            if (!File.Exists(logoName))
                await Utils.I.DownloadFileAsync(loadURL, logoName);
            BitmapImage logo = new BitmapImage {
                CacheOption = BitmapCacheOption.OnLoad
            };
            logo.BeginInit();
            logo.UriSource = new Uri(logoName);
            logo.EndInit();
            caller.Source = logo;
            logo.BaseUri = null;
        }
        void ToggleBtn_Click(object sender, RoutedEventArgs e) {
            try {
                ToggleButton tb = sender as ToggleButton;
                SelectedPoint = apps[toggles.IndexOf(tb)];
                launchBtn.Visibility = Visibility.Visible;
                LibLogo.Visibility = Visibility.Collapsed;
                BitmapImage logoBG = new BitmapImage();
                logoBG.CacheOption = BitmapCacheOption.OnLoad;
                logoBG.BeginInit();
                logoBG.UriSource = new Uri("pack://application:,,,/Images/plain.jpg");
                logoBG.EndInit();
                LibBG.Source = logoBG;
                if (LibLogo.Visibility != Visibility.Hidden && SelectedPoint.Logo != null) {
                    LoadImage(apps[toggles.IndexOf(tb)]);
                    LibLogo.Visibility = Visibility.Visible;
                }
                for (int j = 0; j < toggles.Count; j++) toggles[j].IsChecked = false;
                tb.IsChecked = true;
                if (canEnableLaunchBtn && apps.Count > 0) {
                    if (apps[toggles.IndexOf(tb)].Status.ToString() == "0") launchBtn.IsEnabled = false;
                    else launchBtn.IsEnabled = true;
                    if (apps[toggles.IndexOf(tb)].LoadStatus == false
                        && !apps[toggles.IndexOf(tb)].Custom) launchBtn.Content = locals[0];
                    else launchBtn.Content = locals[1];
                    if (apps[toggles.IndexOf(tb)].NeedsUpdate) updateBtn.Visibility = Visibility.Visible;
                    else updateBtn.Visibility = Visibility.Collapsed;
                }
            }
            catch {
                launchBtn.Visibility = Visibility.Collapsed;
                updateBtn.Visibility = Visibility.Collapsed;
                return;
            }
        }
        void CheckDatabase() {
            string ids = Database.GetAppIDs();
            if (ids != "-") {
                List<XGLApp> tmp = Database.GetXGLApps(ids);
                for (int i = 0; i < apps.Count; i++) {
                    for (int j = 0; j < tmp.Count; j++) {
                        if (apps[i].ID == tmp[j].ID) {
                            tmp[j].LoadStatus = true;
                            if (apps[i].Version != tmp[j].Version)
                                tmp[j].NeedsUpdate = true;
                            tmp[j].Background = tmp[j].Background;
                            tmp[j].Logo = tmp[j].Logo;
                        }
                    }
                }
                apps.Clear();
                if(customApps.Count > 0)
                    apps = tmp.Concat(customApps).ToList();
                else
                    apps= tmp;
                apps.Sort();
            }
        }

        async void GenerateButtons() {
            while(!LocalizationManager.I.LocalLoaded()) await Task.Delay(25);
            for (int i = 0; i < apps.Count; i++) {
                Regex sWhitespace = new Regex(@"\s+");
                ToggleButton btn = new ToggleButton {
                    Style = (Style)TryFindResource(gBtnStyleName),
                    Height = 40,
                    Name = sWhitespace.Replace(GetGoodTBName(apps[i].Name).ToLower() + "tb", string.Empty)
                };
                if (!apps[i].LoadStatus && !apps[i].Custom) btn.Foreground = new SolidColorBrush(Colors.Gray);
                else btn.Foreground = new SolidColorBrush(Colors.White);
                if (apps[i].NeedsUpdate) btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2390e2"));
                string appElName = sWhitespace.Replace(GetGoodName(apps[i]), string.Empty);
                if (sWhitespace.Replace(apps[i].Name, string.Empty).Split('\'').Length > 1)
                    appElName = sWhitespace.Replace(apps[i].Name, string.Empty).Split('\'')[0] +
                        sWhitespace.Replace(apps[i].Name, string.Empty).Split('\'')[1];
                Image miniLogo = new Image {
                    Name = GetGoodTBName(appElName) + "_MiniLibLogo",
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(0, 0.25, 5, 0.25),
                    Width = 25,
                    Height = 25
                };
                if(oldStyle) miniLogo.Visibility = Visibility.Visible;
                TextBlock mainText = new TextBlock {
                    Foreground = btn.Foreground,
                    Text = GetGoodName(apps[i])
                };
                if (apps[i].NeedsUpdate) mainText.Text += LocalizationManager.I.dictionary["mw.g.update"];
                miniLogos.Add(miniLogo);
                texts.Add(mainText);
                btn.Content = new StackPanel {
                    Orientation = Orientation.Horizontal,
                    Children = { miniLogo, mainText }
                };
                btn.MouseRightButtonDown += (object sender, System.Windows.Input.MouseButtonEventArgs e) => {
                    SelectedPoint = apps[toggles.IndexOf(sender as ToggleButton)];
                };
                ContextMenu strip = new ContextMenu();
                MenuItem launchI = new MenuItem { Header = locals[1] };
                if(!apps[i].LoadStatus && !apps[i].Custom) launchI.Header = locals[0];
                launchI.Click += (object sender, RoutedEventArgs e) => LaunchBtn_Click(sender, null);
                if (apps[i].NeedsUpdate) {
                    launchI.Click += (object sender, RoutedEventArgs e) => UpdateBtn_Click(sender, null);
                    launchI.Header = LocalizationManager.I.dictionary["gn.upd"];
                }
                MenuItem launchD = new MenuItem { Header = LocalizationManager.I.dictionary["gn.del"] };
                launchD.Click += (object sender, RoutedEventArgs e) => DeleteApp(SelectedPoint);
                if(apps[i].Status != 0)
                    strip.Items.Add(launchI);
                if(apps[i].LoadStatus || apps[i].Custom)
                    strip.Items.Add(launchD);
                btn.ContextMenu = strip;
                LoadMiniLogo(apps[i], miniLogo);
                btn.Click += ToggleBtn_Click;
                gBarSP.Children.Add(btn);
                toggles.Add(btn);
            }
        }

        string GetGoodName(XGLApp app) {
            if (app.Custom) {
                return app.Name.Split('.')[0];
            }
            else return app.Name;
        }
        string GetGoodTBName(string app) {
            char[] appC = app.ToCharArray();
            char[] appInvC = { '{', '}', '"', '+', '/', '\\', '-', '=', '*', '\'',
                    '.', ',', '?', '!', '@', '#', '$', '$', '%', '^', '&', '(', ')'};
            string result = string.Empty;
            for (int i = 0; i < appC.Length; i++) {
                bool isCValid = true;
                for (int j = 0; j < appInvC.Length; j++) {
                    if (appC[i] == appInvC[j]) isCValid = false;
                    if (j == appInvC.Length - 1 && isCValid) result += appC[i];
                }
            }
            return result;
        }
        void LaunchBtn_Click(object sender, RoutedEventArgs e) {
            if (SelectedPoint.Custom) {
                MainWindow.Manager.Launch(SelectedPoint, SelectedPoint.Path);
                return;
            }
            if(SelectedPoint.LoadStatus) MainWindow.Manager.Launch(SelectedPoint);
            else MainWindow.Manager.Install(SelectedPoint);
        }
        void UpdateBtn_Click(object sender, RoutedEventArgs e) {
            updateBtn.Visibility = Visibility.Visible;
            launchBtn.IsEnabled = false;
            canEnableLaunchBtn = false;
            string termPath = Path.Combine(App.AppsFolder, SelectedPoint.Name);
            if (Directory.Exists(termPath)) {
                DirectoryInfo di = new DirectoryInfo(termPath);
                foreach (FileInfo file in di.GetFiles()) file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                Directory.Delete(termPath);
            }
            DeleteFromGamesFile(SelectedPoint.ID);
            LaunchBtn_Click(launchBtn, e);
            launchBtn.IsEnabled = true;
            canEnableLaunchBtn = true;
        }
        void AddCustomGame(object sender, RoutedEventArgs e) {
            Clear();
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
        public void WriteToGamesFile(string path, bool isCustom = true, string name = "", string version = "1.0", long id = 0) {
            string appsFile = Path.Combine(App.AppsFolder, "Applications");
            if (string.IsNullOrEmpty(name)) name = path.Split('\\')[path.Split('\\').Length - 1];
            string add = $"{id}^{name}^{isCustom}^{version}^{path}\n";
            try {
                File.WriteAllText(appsFile, File.ReadAllText(appsFile) + add);
            }
            catch {
                File.WriteAllText(appsFile, add);
            }
        }
        void ReadFromGamesFile() {
            string appsFile = Path.Combine(App.AppsFolder, "Applications");
            if (File.Exists(appsFile)) {
                string[] applications = File.ReadAllText(appsFile).Split('\n');
                for (int i = 0; i < applications.Length; i++) {
                    if (!string.IsNullOrEmpty(applications[i])) {
                        if(!bool.Parse(applications[i].Split('^')[2]))
                            apps.Add(new XGLApp(long.Parse(applications[i].Split('^')[0]), applications[i].Split('^')[1],
                                applications[i].Split('^')[4], bool.Parse(applications[i].Split('^')[2]),
                                applications[i].Split('^')[3]));
                        else
                            customApps.Add(new XGLApp(long.Parse(applications[i].Split('^')[0]), applications[i].Split('^')[1],
                                applications[i].Split('^')[4], bool.Parse(applications[i].Split('^')[2]),
                                applications[i].Split('^')[3]));
                    }
                }
            } else {
                File.Create(appsFile);
                SelectedPoint = apps.FirstOrDefault();
            }
        }
        void DeleteFromGamesFile(long id) {
            string appsFile = Path.Combine(App.AppsFolder, "Applications");
            if (File.Exists(appsFile)) {
                string[] applications = File.ReadAllText(appsFile).Split('\n');
                string[] newList = new string[applications.Length - 1];
                for (int i = 0; i < applications.Length; i++) {
                    if(!string.IsNullOrEmpty(applications[i])) {
                        if (applications[i].Split('^')[0] != id.ToString())
                            newList[i] = applications[i];
                    }
                }
                File.WriteAllText(appsFile, string.Join("\n", newList));
            }
        }
        void DeleteApp(XGLApp app) {

            if (!app.Custom) if (app == null || !app.LoadStatus) return;

            Clear();
            launchBtn.IsEnabled = false;
            canEnableLaunchBtn = false;
            if (!app.Custom) {
                string termPath = Path.Combine(App.AppsFolder, app.Name);
                if (Directory.Exists(termPath)) {
                    DirectoryInfo di = new DirectoryInfo(termPath);
                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                    Directory.Delete(termPath);
                }
            }
            DeleteFromGamesFile(app.ID);
            launchBtn.IsEnabled = true;
            canEnableLaunchBtn = true;
            Reload();

        }
        
        async void CheckStyling() {
            //Advanced downloads.
            if (RegistrySLS.LoadBool("AdvancedDownloads", false)) {
                LocalizationManager l = LocalizationManager.I;
                while (!l.LocalLoaded()) await Task.Delay(25);
                progress_bar_adv.Cursor = System.Windows.Input.Cursors.Hand;
                progress_bar_adv.Visibility = Visibility.Visible;
                downloaderPB.Visibility = Visibility.Collapsed;
                downloaderPB_game.Text = l.dictionary["mw.g.downloads"];
            } else {
                progress_bar_adv.Visibility= Visibility.Collapsed;
                downloaderPB.Visibility = Visibility.Visible;
            }
            //Change bar status.
            if(!RegistrySLS.LoadBool("GBarStatus", true)) GBarChangeStatus(true);
            else GBarChangeStatus(false);
            //Update visibility of dev elements.
            if (RegistrySLS.LoadBool("GBarAddGame", false))
                addGameBtn.Visibility = Visibility.Visible;
            else addGameBtn.Visibility = Visibility.Collapsed;
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
                gSearchBarTB.Style = (Style)TryFindResource("default");
                addGameBtn.Style = (Style)FindResource("BigButtonGray");
                gBtnStyleName = "GameBtn";
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
                if (toggles[i].Name == sWhitespace.Replace(SelectedPoint.Name.ToLower().Replace("'", string.Empty)
                    .Replace(".", string.Empty) + "tb", string.Empty))
                    tb = toggles[i];
            }
            ToggleBtn_Click(tb, null);
        }
        public void UpdateControls(bool s) { launchBtn.IsEnabled = s; canEnableLaunchBtn = s; }
        public void ApplyLocalization() {
            LocalizationManager l = LocalizationManager.I;
            searchBarT.Text = l.dictionary["mw.search"];
            locals.Clear();
            locals.Add(l.dictionary["mw.download"]);
            locals.Add(l.dictionary["mw.play"]);
            downloaderPB_game.Text = l.dictionary["mw.g.downloads"];
            addApp_T.Text = l.dictionary["mw.g.addapp"];
            if(SelectedPoint != null && toggles.Count > 0)
                ToggleBtn_Click(toggles[apps.IndexOf(SelectedPoint)], null);
        }

        void AdvancedDownloadsView(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if(!RegistrySLS.LoadBool("AdvancedDownloads", false)) return;
            MainWindow.Instance.downloadsControl.Clear();
            MainWindow.Instance.downloadsControl.Reload();
            MainWindow.Instance.CollapseAllPages(MainWindow.Instance.DownloadsPg);

        }
    }
}
