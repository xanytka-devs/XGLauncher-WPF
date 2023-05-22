using Microsoft.Win32;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using XGL.Common;
using XGL.Dialogs.Login;
using XGL.Networking;
using XGL.SLS;

namespace XGL.Dialogs {

    /// <summary>
    /// Логика взаимодействия для OptionsWindow.xaml
    /// </summary>

    public partial class OptionsWindow : Window {

        readonly RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        readonly List<ScrollViewer> pages = new List<ScrollViewer>();
        readonly List<TextBlock> langBtnTexts = new List<TextBlock>();
        bool isChangeCritical = false;
        public static RoutedCommand BetaToggle = new RoutedCommand();

        public OptionsWindow() {
            InitializeComponent();
            pages.Add(generalPage);
            pages.Add(accountPage);
            pages.Add(languagePage);
            pages.Add(appearancePage);
            pages.Add(privacyPage);
            pages.Add(betaPage);
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => { 
                MainWindow.Instance.AllWindows.Remove(this); 
                MainWindow.Instance.Reload();
            };
            Loaded += OptionsWindow_Loaded;
        }

        protected override void OnClosed(EventArgs e) {
            MainWindow.Instance.settingsW = null;
            base.OnClosed(e);
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= OptionsWindow_Loaded;
            //General page.
            if (rkApp.GetValue("XGLauncher") == null) autorunTgB.IsChecked = false;
            else autorunTgB.IsChecked = true;
            g_autostoresearch_TB.IsChecked = RegistrySLS.LoadBool("AutoStoreSearch", false);
            gp_saab_CB.IsChecked = RegistrySLS.LoadBool("GBarAddGame", false);
            gp_mpas_CB.IsChecked = RegistrySLS.LoadBool("SavePosSize", true);
            gp_sfu_CB.IsChecked = RegistrySLS.LoadBool("AutoUpdate", true);
            gp_ar_CB.IsChecked = RegistrySLS.LoadBool("SW_AutoReload", false);
            gp_cs_T.Text = Utils.I.GetDirectorySize(new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"))).ToString();
            gp_dctr_CB.IsChecked = RegistrySLS.LoadBool("DClickToReloadTab", false);
            //Account page.
            pP.Text = Encoding.ASCII.GetString(Base64.Decode(Encoding.ASCII.GetBytes(App.AccountData[1])));
            pHP.Text = string.Empty;
            string pass = string.Empty;
            for(int i = 0; i < pP.Text.Length; i++) {
                pass += "*";
            }
            pHP.Text = pass;
            pN.Text = RegistrySLS.LoadString("LoginData").Split(';')[0];
            eN.Text = Database.GetValue(new Account(App.AccountData[0], App.AccountData[1]), "email").ToString();
            //Language page.
            LoadLangsFromDB();
            ApplyLocalization();
            //Appearance page.
            ap_oldstyle_TB.IsChecked = RegistrySLS.LoadBool("OldStyle", false);
            LoadThemeButtons();
            if(RegistrySLS.LoadBool("Themes", false)) ap_theme_panel.Visibility = Visibility.Visible;
            //Privacy page.
            gp_hit_CB.IsChecked = RegistrySLS.LoadBool("HideInTray", true);
            //Early Access page
            beta_lwos_CB.IsChecked = RegistrySLS.LoadBool("LoginAuth", false);
            beta_scs_CB.IsChecked = RegistrySLS.LoadBool("Community", false);
            beta_uapi_CB.IsChecked = RegistrySLS.LoadBool("UseXGLAPI", true);
            beta_adwnds_CB.IsChecked = RegistrySLS.LoadBool("AdvancedDownloads", false);
            beta_themes_CB.IsChecked = RegistrySLS.LoadBool("Themes", false);
            //Dev page.
            //fd_sps_CB.IsChecked = RegistrySLS.LoadBool("PluginsButton", false);
            //fd_uplg_CB.IsChecked = RegistrySLS.LoadBool("Plugins", false);
            //Hotkeys
            BetaToggle.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            if (RegistrySLS.LoadBool("Beta", false)) betaBtn.Visibility = Visibility.Visible;
            else betaBtn.Visibility = Visibility.Collapsed;
        }

        class LocalBtn {

            public string Name { get; set; }
            public string Abr { get; set; }
            public string Icon { get; set; }
            public Button Button { get; set; }
            public bool LoadStatus { get; set; } = true;

            public LocalBtn(string name, string abr, string icon) {
                Name = name;
                Abr = abr;
                Icon = icon;
            }
        }

        List<LocalBtn> bToGen = new List<LocalBtn>();
        void LoadLangsFromDB() {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `localization_table` WHERE `appID`=1",
                Database.Connection);
            try {
                //Open connection and read everything, what needed.
                Database.OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while (dr.Read()) {
                    bToGen.Add(new LocalBtn(dr.GetString("fullName"),
                                            dr.GetString("lang"),
                                            dr.GetString("langIcon")));
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

            for (int i = 0; i < bToGen.Count; i++) {
                //Check for existance.
                string nameOfFile = Path.Combine(App.CurrentFolder, "localizations",
                    bToGen[i].Abr + ".ini");
                if (!File.Exists(nameOfFile)) bToGen[i].LoadStatus = false;
                //Lang image.
                Image img = new Image {
                    Height = 30,
                    Margin = new Thickness(5), 
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                LoadImage(i, img);
                //Name.
                SolidColorBrush color = new SolidColorBrush(Colors.White);
                if (!bToGen[i].LoadStatus) color = new SolidColorBrush(Colors.Gray);
                TextBlock tb = new TextBlock {
                    Text = bToGen[i].Name,
                    Margin = new Thickness(58, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Style = new Style(),
                    Foreground = color
                };
                langBtnTexts.Add(tb);
                //Holder.
                Grid grid = new Grid {
                    SnapsToDevicePixels = true,
                    Width = 590,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                grid.Children.Add(img);
                grid.Children.Add(tb);
                //Button body.
                Button btn = new Button {
                    Name = "cl_" + bToGen[i].Abr.ToLower().Replace("-", "_"),
                    Height = 40
                };
                bToGen[i].Button = btn;
                btn.Click += ChangeLanguage;
                btn.Content = grid;
                lang_pan.Children.Add(btn);
            }

            string lang = RegistrySLS.LoadString("Language");
            for (int i = 0; i < bToGen.Count; i++)
                if (lang == bToGen[i].Abr) langBtnTexts[i].FontWeight = FontWeights.Bold;

        }

        async void LoadImage(int i, Image source) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", 
                bToGen[i].Icon.Split('/')[bToGen[i].Icon.Split('/').Length - 1]);
            if (!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(bToGen[i].Icon, nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            source.Source = logo;
        }

        void PasswordBtn_Click(object sender, RoutedEventArgs e) {

            switch (pHP.Visibility) {
                case Visibility.Visible:
                    pHP.Visibility = Visibility.Collapsed;
                    pP.Visibility = Visibility.Visible;
                    break;
                default:
                    pHP.Visibility = Visibility.Visible;
                    pP.Visibility = Visibility.Collapsed;
                    break;
            }

        }

        private void SignOutBtn_Click(object sender, RoutedEventArgs e) {

            //TODO: Implement custom dialog system.
            MessageBoxResult soWarnMB = MessageBox.Show(LocalizationManager.I.dictionary["sw.warn.so"], "XGLauncher", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (soWarnMB != MessageBoxResult.Yes) return;
            MainWindow.Instance.SignOut();
            Close();

        }

        private void AutorunTgB_Click(object sender, RoutedEventArgs e) {

            if((bool)autorunTgB.IsChecked) rkApp.SetValue("XGLauncher", System.Reflection.Assembly
                .GetExecutingAssembly().Location);
            else rkApp.DeleteValue("XGLauncher", false);

        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) {

            for (int i = 0; i < pages.Count; i++) {
                pages[i].Visibility = Visibility.Collapsed;
            }
            switch ((sender as FrameworkElement).Name) {
                case "generalBtn":
                    generalPage.Visibility = Visibility.Visible;
                    break;
                case "accountBtn":
                    accountPage.Visibility = Visibility.Visible;
                    break;
                case "languageBtn":
                    languagePage.Visibility = Visibility.Visible;
                    break;
                case "appearanceBtn":
                    appearancePage.Visibility = Visibility.Visible;
                    break;
                case "privacyBtn":
                    privacyPage.Visibility = Visibility.Visible;
                    break;
                case "betaBtn":
                    betaPage.Visibility = Visibility.Visible;
                    break;
            }

        }

        private void ChangeLanguage(object sender, RoutedEventArgs e) {
            object selTB = sender;
            string culture = "en-US";
            for (int i = 0; i < bToGen.Count; i++) {
                if(sender == bToGen[i].Button) {
                    selTB = langBtnTexts[i];
                    culture = bToGen[i].Abr;
                    langBtnTexts[i].Foreground = new SolidColorBrush(Colors.White);
                }
            }
            RegistrySLS.Save("Language", CultureInfo.GetCultureInfo(culture));
            for (int i = 0; i < langBtnTexts.Count; i++) {
                langBtnTexts[i].FontWeight = FontWeights.Regular;
            }
            (selTB as TextBlock).FontWeight = FontWeights.Bold;
            Localize(culture);

        }

        async void Localize(string culture) {
            LocalizationManager.I.LoadLocalization(culture);
            while (!LocalizationManager.I.LocalLoaded()) await Task.Delay(25);
            MainWindow.Instance.ApplyLocalization();
            ApplyLocalization();
        }

        public void ApplyLocalization() {

            LocalizationManager l = LocalizationManager.I;

            //General
            generalBtn.Content = l.dictionary["sw.sv.main"];
            g_ut.Text = l.dictionary["sw.g.utils"];
            g_it.Text = l.dictionary["sw.g.interface"];
            ti_G.Text = l.dictionary["sw.stg"];
            ras_t.Text = l.dictionary["sw.g.auto"];
            gp_cs.Text = l.dictionary["sw.g.cache"];
            gp_cc.Content = l.dictionary["sw.g.ccache"];
            g_autostoresearch.Text = l.dictionary["sw.g.asearch"];
            gp_saab_T.Text = l.dictionary["sw.g.addapp"];
            gp_mpas_T.Text = l.dictionary["sw.g.saveposasize"];
            gp_ar_T.Text = l.dictionary["sw.g.areload"];
            gp_sfu_T.Text = l.dictionary["sw.g.updates"];
            gp_hit_T.Text = l.dictionary["sw.g.hitonclosing"];
            gp_dctr_T.Text = l.dictionary["sw.g.dctr"];
            //Account
            accountBtn.Content = ti_Ac.Text = l.dictionary["sw.sv.acc"];
            ap_u.Text = l.dictionary["sw.ac.log"];
            ap_p.Text = l.dictionary["gn.pass"];
            ap_e.Text = l.dictionary["gn.mail"];
            ap_so.Content = l.dictionary["sw.ac.so"];
            ap_da_text.Text = l.dictionary["sw.ac.del"];
            //Language
            languageBtn.Content = ti_L.Text = l.dictionary["sw.sv.lang"];
            //Appearence
            appearanceBtn.Content = ti_Ap.Text = l.dictionary["sw.sv.app"];
            ap_oldstyle.Text = l.dictionary["sw.ap.old"];
            ap_theme_sys_T.Text = l.dictionary["sw.ap.sys"];
            ap_theme_light_T.Text = l.dictionary["sw.ap.l"];
            ap_theme_dark_T.Text = l.dictionary["sw.ap.d"];
            ap_theme_ohio_T.Text = "Ohio";
            //Privacy
            privacyBtn.Content = ti_Pr.Text = l.dictionary["sw.sv.priv"];
            //Title
            mainST.Text = Title = l.dictionary["sw.stg"];

        }

        void GP_Element_Click(object sender, RoutedEventArgs e) {
            string name = string.Empty;
            ToggleButton el = new ToggleButton();
            switch ((sender as FrameworkElement).Name) {
                case "gp_saab_CB":
                    name = "GBarAddGame";
                    el = gp_saab_CB;
                    break;
                case "gp_mpas_CB":
                    name = "SavePosSize";
                    el = gp_mpas_CB;
                    break;
                case "gp_sfu_CB":
                    name = "AutoUpdate";
                    el = gp_sfu_CB;
                    break;
                case "gp_ar_CB":
                    name = "SW_AutoReload";
                    el = gp_ar_CB;
                    break;
                case "gp_hit_CB":
                    name = "HideInTray";
                    el = gp_hit_CB;
                    break;
                case "gp_dctr_CB":
                    name = "DClickToReloadTab";
                    el = gp_dctr_CB;
                    break;
            }
            if (!isChangeCritical) {
                RegistrySLS.Save(name, (bool)el.IsChecked);
                if(RegistrySLS.LoadBool("SW_AutoReload", false)) MainWindow.Instance.Reload();
                return;
            }
            //TODO: Implement custom dialog system.
            MessageBoxResult result = MessageBox.Show(LocalizationManager.I.dictionary["sw.warn.crit"],
                "XGLauncher", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
            if(result == MessageBoxResult.OK) {
                RegistrySLS.Save(name, (bool)el.IsChecked);
                MainWindow.Instance.CloseAllWindows();
                Process.Start(Path.Combine(Environment.CurrentDirectory, "XGLauncher.exe"));
            } else {
                el.IsChecked = !el.IsChecked;
            }
        }

        void RebootLauncher(object sender, RoutedEventArgs e) => MainWindow.Instance.Reload();

        void DeleteAccount_Click(object sender, RoutedEventArgs e) {
            //TODO: Implement custom dialog system.
            MessageBoxResult result = MessageBox.Show(LocalizationManager.I.dictionary["sw.warn.del"],
                "XGLauncher", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.OK) {
                MessageBox.Show(LocalizationManager.I.dictionary["sw.warn.del.comp"], "XGLauncher", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Database.SetValue(Database.DBDataType.DT_ACTIVITY, 9);
                MainWindow.Instance.CloseAllWindows();
                App.RunMySQLCommands = false;
                RegistrySLS.Reset();
                App.RunMySQLCommands = true;
            }
        }

        void ClearCache(object sender, RoutedEventArgs e) {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"));
            foreach (FileInfo file in di.GetFiles()) { if(!Utils.I.IsFileLocked(file)) file.Delete(); }
            foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
            gp_cs_T.Text = Utils.I.GetDirectorySize(new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"))).ToString();
        }

        void AutoStoreSearch_Click(object sender, RoutedEventArgs e) { 
            RegistrySLS.Save("AutoStoreSearch", !RegistrySLS.LoadBool("AutoStoreSearch", false));
        }

        void OldStyle_Click(object sender, RoutedEventArgs e) { 
            RegistrySLS.Save("OldStyle", (bool)ap_oldstyle_TB.IsChecked);
            MainWindow.Instance.curP = 0;
            MainWindow.Instance.MainBtn_Click(MainWindow.Instance.games, null);
            MainWindow.Instance.Reload();
            MainWindow.Instance.gamesControl.Clear();
            MainWindow.Instance.gamesControl.Reload();
            MainWindow.Instance.curP = 2;
        }

        void ThemeBtn_Click(object sender, RoutedEventArgs e) {
            switch ((sender as Button).Name) {
                case "ap_theme_sys":
                    RegistrySLS.Save("Theme", "System");
                    break;
                case "ap_theme_dark":
                    RegistrySLS.Save("Theme", "Dark");
                    break;
                case "ap_theme_light":
                    RegistrySLS.Save("Theme", "Light");
                    break;
                case "ap_theme_ohio":
                    RegistrySLS.Save("Theme", "Ohio");
                    break;
            }
            LoadThemeButtons();
            MainWindow.Instance.ReloadTheme();
            MainWindow.Instance.ReloadTheme();
        }

        void LoadThemeButtons() {
            ap_theme_sys_bg.Opacity = 0;
            ap_theme_dark_bg.Opacity = 0;
            ap_theme_light_bg.Opacity = 0;
            ap_theme_ohio_bg.Opacity = 0;
            switch (RegistrySLS.LoadString("Theme", "System")) {
                case "System":
                    ap_theme_sys_bg.Opacity = 0.125;
                    break;
                case "Dark":
                    ap_theme_dark_bg.Opacity = 0.125;
                    break;
                case "Light":
                    ap_theme_light_bg.Opacity = 0.125;
                    break;
                case "Ohio":
                    ap_theme_ohio_bg.Opacity = 0.125;
                    break;
            }
        }

        private void B_Element_Click(object sender, RoutedEventArgs e) {
            string name = string.Empty;
            ToggleButton el = new ToggleButton();
            switch ((sender as FrameworkElement).Name) {
                case "beta_lwos_CB":
                    name = "LoginAuth";
                    el = beta_lwos_CB;
                    break;
                case "beta_scs_CB":
                    name = "Community";
                    el = beta_scs_CB;
                    break;
                case "beta_uapi_CB":
                    name = "UseXGLAPI";
                    el = beta_uapi_CB;
                    break;
                case "beta_adwnds_CB":
                    name = "AdvancedDownloads";
                    el = beta_adwnds_CB;
                    break;
                case "beta_themes_CB":
                    name = "Themes";
                    el = beta_themes_CB;
                    isChangeCritical = true;
                    break;
            }
            if (!isChangeCritical) {
                RegistrySLS.Save(name, (bool)el.IsChecked);
                if (RegistrySLS.LoadBool("SW_AutoReload", false)) MainWindow.Instance.Reload();
                return;
            }
            //TODO: Implement custom dialog system.
            MessageBoxResult result = MessageBox.Show(LocalizationManager.I.dictionary["sw.warn.crit"],
                "XGLauncher", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
            if (result == MessageBoxResult.OK) {
                RegistrySLS.Save(name, (bool)el.IsChecked);
                MainWindow.Instance.CloseAllWindows();
                Process.Start(Path.Combine(Environment.CurrentDirectory, "XGLauncher.exe"));
            } else
                el.IsChecked = !el.IsChecked;
        }

        void BetaToggled(object sender, ExecutedRoutedEventArgs e) {

            RegistrySLS.Save("Beta", !RegistrySLS.LoadBool("Beta", false));
            if (RegistrySLS.LoadBool("Beta", false)) betaBtn.Visibility = Visibility.Visible;
            else betaBtn.Visibility = Visibility.Collapsed;

        }
    }

}
