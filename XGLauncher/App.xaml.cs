using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
//using XGL.API;
using XGL.Dialogs.Login;
using XGL.Networking;
using XGL.SLS;
using Forms = System.Windows.Forms;

namespace XGL {

    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>

    public partial class App : Application {

        public static bool LoginDataNotSaved = false; //Is password and login saved
        public static string CurrentVersion { get; private set; } = "0.1.6"; //Version
        public static string[] AccountData; // TMP / Raw account data. Soon will be replaced by token
        public static string CurrentFolder { get; private set; } //Application dir
        public static string AppDataFolder { get; private set; }//Application data dir
        public static string AppsFolder { get; private set; } //Apps dir
        public static Account CurrentAccount; //Current account. Soon will be replaced by token
        public static bool RunMySQLCommands { get; set; } = true; //Consider running MySQL commands
        public static string WebclientKey { get; private set; } //Webclient acess key
        public static bool IsFirstRun { get; set; } = false; // TMP / Is programm only started
        public static bool OfflineMode { get; set; } = false; //Offline mode
        public static bool IsPremium { get; set; } = false; //Premium
        public static Forms.NotifyIcon TrayIcon { get; set; } //Icon for idle state in tray
        public static string DBConnectorData { get; private set; } // TMP //
        public static string VKConnectorData { get; private set; } // TMP //
        public static string GoogleCS { get; private set; } // TMP //
        public static string GoogleCID { get; private set; } // TMP //
        public static string NREmailKey { get; private set; } // TMP //

        protected override void OnStartup(StartupEventArgs e) {
            //Check if offline mode
            if(e.Args.Contains("-offline")) OfflineMode = true;
            //Instantiate variables
            InstanceVars();
            RegistrySLS.Setup();
            //Check for XGLAPI status
            //FIX: Not working. Package indeficator not found
            /*if(RegistrySLS.LoadBool("UseXGLAPI", true))
                Core.Main(e.Args.ToArray());*/
            //Check folders and continue launching
            FolderCheck();
            //Check for system language
            if(RegistrySLS.LoadString("Language", "INS") == "INS")
                RegistrySLS.Save("Language", CultureInfo.CurrentCulture);
            if(!OfflineMode) {
                //Check online status and instantiate account
                CheckStatus();
                if(AccountData.Length > 1 && AccountData[0] != "INS" && AccountData[0] != "False")
                    CurrentAccount = new Account(AccountData[0], AccountData[1]);
                //Check account status
                AccountCheck();
            }

            base.OnStartup(e);
        }

        void AccountCheck() {
            if(!LoginDataNotSaved) {
                //Try check connection
                if(!Database.TryOpenConnection()) {
                    LoginWindow l = new LoginWindow();
                    l.Show();
                    return;
                }
                //Check for updates
                if(RegistrySLS.LoadBool("AutoUpdate", true)) Update();
                //Check for account existance
                if(Database.AccountExisting(CurrentAccount)) {
                    if(Database.AccountEmailVerified(CurrentAccount)) {
                        NextWindow();
                        return;
                    }
                }
                LoginWindow lw = new LoginWindow();
                lw.Show();
                return;
            } else {
                LoginWindow l = new LoginWindow();
                l.Show();
            }
        }

        void Update() {
            //Search for update
            string output = string.Empty;
            MySqlCommand command = new MySqlCommand($"SELECT `latest` FROM `applications` WHERE `id` = 1", Database.Connection);
            try {
                Database.OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while(dr.Read()) output = dr.GetString("latest");
                dr.Close();
                Database.CloseConnection();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally { Database.CloseConnection(); }
            //Update if needed
            if(output.Split('{')[0] != CurrentVersion) {
                ProcessStartInfo pr = new ProcessStartInfo() {
                    FileName = $"{CurrentFolder}\\XGLauncher Updater.exe",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                if(File.Exists(pr.FileName))
                    Process.Start(pr);
                Shutdown();
                return;
            }
        }

        void NextWindow() {
            if(Utils.I.NotEmptyAndNotINS(CurrentAccount.Password)) {
                //Save username and password to settings.
                RegistrySLS.Save("LastID", Database.GetID(CurrentAccount));
                Database.SetValue(Database.DBDataType.DT_ACTIVITY, 1);
            }
            //Opens the launcher.
            MainWindow m = new MainWindow();
            m.Show();
        }

        private void InstanceVars() {
            CurrentFolder = Environment.CurrentDirectory;
            AppDataFolder = Path.Combine(CurrentFolder, "ApplicationData");
            AppsFolder = Path.Combine(CurrentFolder, "apps");
            CurrentAccount = new Account("INS", "INS");
            AccountData = RegistrySLS.LoadString("LoginData", "INS;INS").Split(';');
            RegistrySLS.Save("Path", CurrentFolder);
            RegistrySLS.Save("Version", CurrentVersion);
            string[] appSData = INTERNAL.ApplicationSData.IndefData;
            DBConnectorData = appSData[0];
            GoogleCID = appSData[1];
            GoogleCS = appSData[2];
            VKConnectorData = appSData[3];
            NREmailKey = appSData[4];
            WebclientKey = appSData[5];
            new LocalizationManager();
            new Utils();
        }

        private void CheckStatus() => LoginDataNotSaved = Utils.I.NotEmptyAndNotINS(CurrentAccount.Login) && Utils.I.NotEmptyAndNotINS(CurrentAccount.Password);

        protected override void OnExit(ExitEventArgs e) {
            //Check online status.
            CheckStatus();
            //If online - change activity and lastOnline.
            if(!RunMySQLCommands || !LoginDataNotSaved) return;
            Database.SetValue(Database.DBDataType.DT_ACTIVITY, 0);
            Database.SetValue(Database.DBDataType.DT_LASTONLINE, DateTime.Now);
        }

        void FolderCheck() {
            //Instance folders
            if(!Directory.Exists(Path.Combine(CurrentFolder, "cache"))) {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(CurrentFolder, "cache"));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            if(!Directory.Exists(Path.Combine(CurrentFolder, "logs")))
                Directory.CreateDirectory(Path.Combine(CurrentFolder, "logs"));
            if(!Directory.Exists(Path.Combine(CurrentFolder, "apps")))
                Directory.CreateDirectory(Path.Combine(CurrentFolder, "apps"));
            if(!Directory.Exists(Path.Combine(CurrentFolder, "localizations")))
                Directory.CreateDirectory(Path.Combine(CurrentFolder, "localizations"));
            //Load localization
            LocalizationManager.I.LoadLocalization(RegistrySLS.LoadString("Language", RegistrySLS.LoadString("Language", CultureInfo.CurrentCulture.Name)));
        }

    }
}
