using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XGL;
using XGL.Networking;
using XGL.SLS;
using static XGL.Networking.Database;

namespace XGL.Dev {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {

        public static string CurrentVersion { get; private set; } = "0.1";
        public static string[] AccountData;
        public static string CurrentFolder { get; private set; }
        public static string AppDataFolder { get; private set; }
        public static string AppsFolder { get; private set; }
        public static Account CurrentAccount;
        public static bool RunMySQLCommands { get; set; } = true;

        protected override void OnStartup(StartupEventArgs e) {
            //Instantiate variables.
            InstanceVars();
            RegistrySLS.Setup();
            //Read account data.
            if (AccountData.Length > 1 && AccountData[0] != "INS" && AccountData[0] != "False") {
                CurrentAccount = new Account(AccountData[0], AccountData[1]);
            }
            //Check for system language.
            if (RegistrySLS.LoadXGLString("Language", "INS") == "INS") {
                RegistrySLS.Save("Language", CultureInfo.CurrentCulture);
            }
            //Check folders and continue launching.
            FolderCheck();
            base.OnStartup(e);

        }

        private void InstanceVars() {
            CurrentFolder = Environment.CurrentDirectory;
            RegistrySLS.Save("Path", Environment.CurrentDirectory);
            AppDataFolder = Path.Combine(CurrentFolder, "ApplicationData");
            AppsFolder = Path.Combine(CurrentFolder, "apps");
            CurrentAccount = new Account("Not Set", "Not Set");
            AccountData = RegistrySLS.LoadString("PublisherLoginData", "INS;INS").Split(';');
            RegistrySLS.Save("DevToolsVersion", CurrentVersion);
        }

        void FolderCheck() {
            //Create cache folder, if it doesn't exist.
            /*if (!Directory.Exists(Path.Combine(CurrentFolder, "cache"))) {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(CurrentFolder, "cache"));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }*/
            //Create logs folder, if it doesn't exist.
            if (!Directory.Exists(Path.Combine(CurrentFolder, "logs")))
                Directory.CreateDirectory(Path.Combine(CurrentFolder, "logs"));
            //Create cache folder, if it doesn't exist.
            /*if (!Directory.Exists(Path.Combine(CurrentFolder, "localizations")))
                Directory.CreateDirectory(Path.Combine(CurrentFolder, "localizations"));*/
        }

    }
}
