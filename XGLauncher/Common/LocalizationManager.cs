using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using XGL.Networking;

namespace XGL {

    public class LocalizationManager {

        public static LocalizationManager I;
        public LocalizationManager() { I = this; dictionary = new Dictionary<string, string>(); }
        public Dictionary<string, string> dictionary;
        bool LocalizationLoaded = false;

        public void LoadLocalization(string lang) {
            LocalizationLoaded = false;
            if (!Directory.EnumerateFileSystemEntries(Path.Combine(App.CurrentFolder, "localizations")).Contains(lang + ".ini")) {
                if (!Database.TryOpenConnection()) return;
                if (string.IsNullOrEmpty(lang)) return;
                dictionary.Clear();
                MySqlCommand command = new MySqlCommand($"SELECT * FROM `localization_table` WHERE `appID` = 1 and `lang` = '{lang}'", Database.Connection);
                string URL = string.Empty;
                try {
                    Database.OpenConnection();
                    MySqlDataReader dr;
                    dr = command.ExecuteReader();
                    while (dr.Read()) URL = dr.GetString("url");
                    dr.Close();
                    Database.CloseConnection();
                    if (!string.IsNullOrEmpty(URL) && URL != "INS")
                        Download(URL.Split('{')[1], lang);
                }
                catch (Exception ex) {
                    //TODO: Implement custom dialog system.
                    MessageBox.Show(ex.Message);
                    throw;
                }
            } else CreateDictionary(Path.Combine(App.CurrentFolder, "localizations", lang + ".ini"));

        }

        async void Download(string url, string lang) {
            if (!App.RunMySQLCommands || App.CurrentAccount == null) return;
            string nameOfFile = Path.Combine(App.CurrentFolder, "localizations", lang + ".ini");
            if (!File.Exists(nameOfFile))
                await Utils.I.DownloadFileAsync(url, nameOfFile);
            CreateDictionary(nameOfFile);
        }

        public void CreateDictionary(string p) {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            string[] l = File.ReadAllText(p).Split('\n');
            for(int i = 0; i < l.Length; i++) {
                if(l[i].StartsWith("#")) continue;
                tmp.Add(l[i].Split('=')[0], l[i].Split('=')[1].Split('\r')[0]);
            }
            dictionary = tmp;
            LocalizationLoaded = true;
        }

        public bool LocalLoaded() { return LocalizationLoaded; }

    }

}
