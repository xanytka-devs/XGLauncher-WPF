using Microsoft.Win32;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Linq;
using XGL.Common;
using XGL.Dialogs.Login;
using XGL.Networking.Database;
using XGL.SLS;

namespace XGL.Dialogs {

    /// <summary>
    /// Логика взаимодействия для OptionsWindow.xaml
    /// </summary>

    public partial class OptionsWindow : Window {

        readonly RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        List<string> localizes = new List<string>();
        readonly List<ScrollViewer> pages = new List<ScrollViewer>();
        readonly List<TextBlock> langBtnTexts = new List<TextBlock>();
        bool isChangeCritical = false;

        public OptionsWindow() {
            InitializeComponent();
            pages.Add(generalPage);
            pages.Add(accountPage);
            pages.Add(languagePage);
            pages.Add(appearancePage);
            pages.Add(aboutPage);
            pages.Add(devPage);
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
            fd_saab_CB.IsChecked = RegistrySLS.LoadBool("GBarAddGame", false);

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
            langBtnTexts.Add(cl_ru_ru_tb);
            langBtnTexts.Add(cl_en_us_tb);
            langBtnTexts.Add(cl_es_tb);
            langBtnTexts.Add(cl_ru_im_tb);
            string lang = RegistrySLS.LoadString("Language");
            if (lang == "ru-RU") cl_ru_ru_tb.FontWeight = FontWeights.Bold;
            if (lang == "en-US") cl_en_us_tb.FontWeight = FontWeights.Bold;
            if (lang == "es") cl_es_tb.FontWeight = FontWeights.Bold;
            if (lang == "ru-IM") cl_ru_im_tb.FontWeight = FontWeights.Bold;
            ApplyLocalization(lang);

            //Appearance page.
            ap_oldstyle_TB.IsChecked = RegistrySLS.LoadBool("OldStyle", false);
            LoadThemeButtons();

            //About page.
            verT.Text = App.CurrentVersion;

            //Dev page.
            fd_lwos_CB.IsChecked = RegistrySLS.LoadBool("LoginAuth", false);
            fd_scs_CB.IsChecked = RegistrySLS.LoadBool("Community", false);
            fd_sps_CB.IsChecked = RegistrySLS.LoadBool("PluginsButton", false);
            fd_uapi_CB.IsChecked = RegistrySLS.LoadBool("UseXGLAPI", true);
            fd_uplg_CB.IsChecked = RegistrySLS.LoadBool("Plugins", false);
            fd_ard_CB.IsChecked = RegistrySLS.LoadBool("DevAutoReload", false);
            fd_sfu_CB.IsChecked = RegistrySLS.LoadBool("AutoUpdate", true);
            fd_ar_CB.IsChecked = RegistrySLS.LoadBool("SW_AutoReload", false);
            //Additional Dev page checks.
            if (!(bool)fd_uplg_CB.IsChecked) {
                fd_sps_CB.IsEnabled = false;
                fd_sps_T.IsEnabled = false;
                fd_sps_T.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
            fd_reload_btn.IsEnabled = !RegistrySLS.LoadBool("DevAutoReload", false);
            gp_cs_T.Text = Utils.GetDirectorySize(new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"))).ToString() + " " + gp_cs_T.Text;

            if (int.Parse(Database.GetValue(App.CurrentAccount, "publicRights").ToString()) != 1) devBtn.Visibility = Visibility.Collapsed;

        }

        private void PasswordBtn_Click(object sender, RoutedEventArgs e) {

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
            MessageBoxResult soWarnMB = MessageBox.Show("Are you sure you want to sign out?", "XGLauncher", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (soWarnMB == MessageBoxResult.Yes) { 
                MainWindow.Instance.SignOut();
                Close();
            }

        }

        private void AutorunTgB_Click(object sender, RoutedEventArgs e) {

            if((bool)autorunTgB.IsChecked) rkApp.SetValue("XGLauncher", System.Reflection.Assembly.GetExecutingAssembly().Location);
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
                case "aboutBtn":
                    aboutPage.Visibility = Visibility.Visible;
                    break;
                case "devBtn":
                    devPage.Visibility = Visibility.Visible;
                    break;
            }

        }

        private void ChangeLanguage(object sender, RoutedEventArgs e) {
            object selTB = sender;
            string culture = "en-US";

            if(sender == cl_ru_ru) {
                selTB = cl_ru_ru_tb;
                culture = "ru-RU";
            } else if (sender == cl_en_us) {
                selTB = cl_en_us_tb;
                culture = "en-US";
            } else if (sender == cl_es) {
                selTB = cl_es_tb;
                culture = "es";
            } else if (sender == cl_ru_im) {
                selTB = cl_ru_im_tb;
                culture = "ru-IM";
            }

            RegistrySLS.Save("Language", CultureInfo.GetCultureInfo(culture));
            for (int i = 0; i < langBtnTexts.Count; i++) {
                langBtnTexts[i].FontWeight = FontWeights.Regular;
            }
            (selTB as TextBlock).FontWeight = FontWeights.Bold;
            MainWindow.Instance.ApplyLocalization(culture);
            localizes.Clear();
            ApplyLocalization(culture);

        }

        public void ApplyLocalization(string localization) {

            switch (localization) {
                case "ru-RU":
                    localizes.Add("Основное");
                    localizes.Add("Аккаунт");
                    localizes.Add("Язык");
                    localizes.Add("Внешний вид");
                    localizes.Add("Информация");
                    localizes.Add("Настройки");
                    localizes.Add("Запуск при включении системы");
                    localizes.Add("Имя пользователя");
                    localizes.Add("Пароль");
                    localizes.Add("Почта");
                    localizes.Add("Выйти");
                    localizes.Add("Удалить Аккаунт");
                    localizes.Add("Кэш");
                    localizes.Add("Очистить кэш");
                    localizes.Add("бит");
                    localizes.Add("Автоматически искать товары в магазине");
                    localizes.Add("Показывать кнопку 'Добавить приложение' в Библиотеке");
                    localizes.Add("Интерфейс");
                    localizes.Add("Утилиты");
                    localizes.Add("Автоматически перезагружать при изменении настроек");
                    localizes.Add("Автоматически искать обновления");
                    localizes.Add("Старый стиль");
                    localizes.Add("Системная");
                    localizes.Add("Светлая");
                    localizes.Add("Тёмная");
                    localizes.Add("Охае");
                    break;
                case "en-US":
                    localizes.Add("General");
                    localizes.Add("Account");
                    localizes.Add("Language");
                    localizes.Add("Appearance");
                    localizes.Add("About");
                    localizes.Add("Settings");
                    localizes.Add("Run at start of system");
                    localizes.Add("Username");
                    localizes.Add("Password");
                    localizes.Add("Email");
                    localizes.Add("Sign out");
                    localizes.Add("Delete Account");
                    localizes.Add("Cache");
                    localizes.Add("Clear cache");
                    localizes.Add("bits");
                    localizes.Add("Automatically search for products in shop");
                    localizes.Add("Show 'Add application' button in Library");
                    localizes.Add("Interface");
                    localizes.Add("Utilities");
                    localizes.Add("Automatically reload on setting changed");
                    localizes.Add("Automatically search for updates");
                    localizes.Add("Old style");
                    localizes.Add("System");
                    localizes.Add("Light");
                    localizes.Add("Dark");
                    localizes.Add("Ohio");
                    break;
                case "es":
                    localizes.Add("General");
                    localizes.Add("Cuenta");
                    localizes.Add("Idioma");
                    localizes.Add("Apariencia");
                    localizes.Add("Sobre");
                    localizes.Add("Configuración");
                    localizes.Add("Ejecutar al inicio del sistema");
                    localizes.Add("Nombre de usuario");
                    localizes.Add("Contraseña");
                    localizes.Add("Correo");
                    localizes.Add("Salir");
                    localizes.Add("Eliminar Cuenta");
                    localizes.Add("Cache");
                    localizes.Add("Borrar caché");
                    localizes.Add("bits");
                    localizes.Add("Buscar automáticamente productos en la tienda");
                    localizes.Add("Mostrar el botón 'Agregar aplicación' en la Biblioteca");
                    localizes.Add("Interfaz");
                    localizes.Add("Utilidades");
                    localizes.Add("Reiniciar automáticamente cuando cambie la configuración");
                    localizes.Add("Buscar actualizaciones automáticamente");
                    localizes.Add("Estilo antiguo");
                    localizes.Add("Sistémico");
                    localizes.Add("Luminoso");
                    localizes.Add("Oscuro");
                    localizes.Add("Ohio");
                    break;
                case "ru-IM":
                    localizes.Add("Основное");
                    localizes.Add("Аккаунтъ");
                    localizes.Add("Языкъ");
                    localizes.Add("Внѣшнiй видъ");
                    localizes.Add("Информація");
                    localizes.Add("Настройки");
                    localizes.Add("Запускъ при включеніи системы");
                    localizes.Add("Имя пользователя");
                    localizes.Add("Пароль");
                    localizes.Add("Почта");
                    localizes.Add("Выйти");
                    localizes.Add("Удалить Аккаунтъ");
                    localizes.Add("Кэшъ");
                    localizes.Add("Очистить ​кэшъ​");
                    localizes.Add("битъ​");
                    localizes.Add("Автоматически искать товары въ магазинѣ​");
                    localizes.Add("Показывать кнопку 'Добавить приложеніе' въ Библіотекѣ ​");
                    localizes.Add("Интерфейсъ");
                    localizes.Add("Утилиты");
                    localizes.Add("Автоматически перезагружать при измѣненіи ​настроекъ");
                    localizes.Add("Автоматически искать обновленія");
                    localizes.Add("Ветхій видъ");
                    localizes.Add("Системная");
                    localizes.Add("Свѣтлая");
                    localizes.Add("Темная");
                    localizes.Add("​Охае​");
                    break;
            }

            //General
            generalBtn.Content = localizes[0];
            g_ut.Text = localizes[18];
            g_it.Text = localizes[17];
            ti_G.Text = localizes[0];
            ras_t.Text = localizes[6];
            gp_cs.Text = localizes[12];
            gp_cc.Content = localizes[13];
            gp_cs_T.Text = localizes[14];
            g_autostoresearch.Text = localizes[15];
            fd_saab_T.Text = localizes[16];
            fd_ar_T.Text = localizes[19];
            fd_sfu_T.Text = localizes[20];
            //Account
            accountBtn.Content = localizes[1];
            ti_Ac.Text = localizes[1];
            ap_u.Text = localizes[7];
            ap_p.Text = localizes[8];
            ap_e.Text = localizes[9];
            ap_so.Content = localizes[10];
            ap_da_text.Text = localizes[11];
            //Language
            languageBtn.Content = localizes[2];
            ti_L.Text = localizes[2];
            //Appearence
            appearanceBtn.Content = localizes[3];
            ti_Ap.Text = localizes[3];
            ap_oldstyle.Text = localizes[21];
            ap_theme_sys_T.Text = localizes[22];
            ap_theme_light_T.Text = localizes[23];
            ap_theme_dark_T.Text = localizes[24];
            ap_theme_ohio_T.Text = localizes[25];
            //About
            aboutBtn.Content = localizes[4];
            ti_Ab.Text = localizes[4];
            //Title
            mainST.Text = localizes[5];
            Title = localizes[5];

        }

        void FD_Element_Click(object sender, RoutedEventArgs e) {
            string name = string.Empty;
            ToggleButton el = new ToggleButton();
            switch ((sender as FrameworkElement).Name) {
                case "fd_lwos_CB":
                    name = "LoginAuth";
                    el = fd_lwos_CB;
                    break;
                case "fd_scs_CB":
                    name = "Community";
                    el = fd_scs_CB;
                    break;
                case "fd_sps_CB":
                    name = "PluginsButton";
                    el = fd_sps_CB;
                    break;
                case "fd_saab_CB":
                    name = "GBarAddGame";
                    el = fd_saab_CB;
                    break;
                case "fd_uapi_CB":
                    name = "UseXGLAPI";
                    el = fd_uapi_CB;
                    isChangeCritical = true;
                    break;
                case "fd_uplg_CB":
                    name = "Plugins";
                    el = fd_uplg_CB;
                    isChangeCritical = true;
                    break;
                case "fd_ard_CB":
                    name = "DevAutoReload";
                    el = fd_ard_CB;
                    break;
                case "fd_sfu_CB":
                    name = "AutoUpdate";
                    el = fd_sfu_CB;
                    break;
                case "fd_ar_CB":
                    name = "SW_AutoReload";
                    el = fd_ar_CB;
                    break;
            }
            if (!isChangeCritical) {
                RegistrySLS.Save(name, (bool)el.IsChecked);
                if(RegistrySLS.LoadBool("SW_AutoReload", false)) MainWindow.Instance.Reload();
                return;
            }
            //TODO: Implement custom dialog system.
            MessageBoxResult result = MessageBox.Show("Some of selected settings are critical. Restart needed to them to apply. Continue?",
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
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your XanID account? You'll completely lose access to it!",
                "XGLauncher", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.OK) {
                MessageBox.Show("Your XanID account was transferred to stasis. To restore it please write a letter to support@xan.ru",
                "XGLauncher", MessageBoxButton.OK, MessageBoxImage.Information);
                Database.SetValue(Database.DBDataType.DT_ACTIVITY, 9);
                MainWindow.Instance.CloseAllWindows();
                App.RunMySQLCommands = false;
                RegistrySLS.Reset();
                App.RunMySQLCommands = true;
            }
        }

        void ClearCache(object sender, RoutedEventArgs e) {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"));
            foreach (FileInfo file in di.GetFiles()) { if(!Utils.IsFileLocked(file)) file.Delete(); }
            foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
            gp_cs_T.Text = Utils.GetDirectorySize(new DirectoryInfo(Path.Combine(App.CurrentFolder, "cache"))).ToString() + " " + localizes[14];
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
                    MessageBox.Show("This theme isn't implemented. Dark one will be used.", "XGLauncher", MessageBoxButton.OK);
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

    }

}
