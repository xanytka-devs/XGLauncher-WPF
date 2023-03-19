using Microsoft.Toolkit.Uwp.Notifications;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using XGL.Common;
using XGL.Networking.Google.Drive;
using XGL.Pages.LW;
using static XGL.Networking.Database.Database;

namespace XGL.Networking {

    public class VersionManager {

        Game _game;
        XGLApp app;
        string _name;
        string _archiveType;
        string link;
        string databaseText;
        string appsDirectory;
        string latestExecutablePath;

        public string[] AvaibleProducts { get; private set; }

        //public VersionManager() { }

        public bool GameExist(string name) {
            string appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            string appFile = Path.Combine(appsDirectory, name, name + ".exe");
            return Directory.Exists(appsDirectory) && File.Exists(appFile);
        }

        public bool IsOutDated(string name) {

            string version = string.Empty;

            //Initialize components.
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `xgl_products` WHERE `name`=" + '"' + name + '"', Connection);
            try {
                //Fill adapter.
                adapter.SelectCommand = command;
                adapter.Fill(table);
                //Open connection and read everything, what needed.
                OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while (dr.Read()) {
                    databaseText = dr.GetString("latestDownloadLinks");
                    version = databaseText.Split('{')[0];
                    link = databaseText.Split('{')[1];
                }
                //Close reader and connection.
                dr.Close();
                CloseConnection();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
                return false;
            }
            string appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            string gameDirectory = Path.Combine(appsDirectory, name);
            //Check for existance of save file.
            if (GameExist(name)) {
                string[] installedVersions = Directory.EnumerateDirectories(gameDirectory).ToArray();
                if (installedVersions.Count() > 0) {
                    if (installedVersions.Last().Split("\\".ToCharArray().Last()).Last() != name + $"({version})") {
                        //If version of game is older than last, return true.
                        return true;
                    } else {
                        //Else, it isn't outdated, so return false
                        return false;
                    }
                }
            }
            return false;

        }

        string GetLatestLink(string name) {
            string version = string.Empty;
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `xgl_products` WHERE `name`=" + '"' + name + '"', Connection);
            try {
                //Fill adapter.
                adapter.SelectCommand = command;
                adapter.Fill(table);
                //Open connection and read everything, what needed.
                OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while (dr.Read()) {
                    databaseText = dr.GetString("latestDownloadLinks");
                    version = databaseText.Split('{')[0];
                    link = databaseText.Split('{')[1];
                }
                //Close reader and connection.
                dr.Close();
                CloseConnection();
                return version;
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void Launch(XGLApp game, string archiveType = "zip", string path = "") {
            if (game.Custom) {
                Process.Start(path);
                return;
            }
            //Define variables.
            _game = new Game(game.Name);
            app = game;
            _name = game.Name;
            _archiveType = archiveType;
            appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            latestExecutablePath = Path.Combine(appsDirectory, _name, _name + ".exe");
            //Check for existance of save file.
            if (GameExist(game.Name)) {
                Process.Start(latestExecutablePath);
                return;
            }
            //If not - download it.
            try {
                //Start downloading.
                GetLatestLink(_name);
                MainWindow.Instance.gamesControl.downloaderPB.Visibility = Visibility.Visible;
                MainWindow.Instance.gamesControl.DisableControls();
                GDFileDownloader downloader = new GDFileDownloader();
                downloader.DownloadProgressChanged += (object sender, GDFileDownloader.DownloadProgress progress) => {
                    MainWindow.Instance.gamesControl.downloaderPB.Value = progress.ProgressPercentage;
                };
                downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                downloader.DownloadFileAsync(link, Path.Combine(appsDirectory, $"{_name}.{archiveType}"));
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            try {
                MainWindow.Instance.gamesControl.EnableControls();
                //Extract files from zip.
                ZipFile.ExtractToDirectory(Path.Combine(appsDirectory, $"{_name}.{_archiveType}"), Path.Combine(appsDirectory, _name));
                File.Delete(Path.Combine(appsDirectory, $"{_name}.{_archiveType}"));
                //Send notification.
                new ToastContentBuilder()
                    .AddArgument("action", "openLauncher")
                    .AddText(_name + " downloaded")
                    .AddText("Now you can try it out!")
                    .Show(toast => {
                        toast.ExpirationTime = DateTime.Now.AddMinutes(10);
                        toast.Activated += (Windows.UI.Notifications.ToastNotification _sender, object args) => MainWindow.Instance.gamesControl.OpenPageOf(app);
                    });
                MainWindow.Instance.gamesControl.downloaderPB.Value = 0;
                MainWindow.Instance.gamesControl.downloaderPB.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                Debug.WriteLine(ex.Message);
            }
        }


        /*private bool LaunchWithoutChecks(string name) {


            return false;
        }*/

    }


}

namespace XGL.Networking.Google.Drive
{



}
 