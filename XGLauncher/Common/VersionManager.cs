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
using XGL.Networking;
using XGL.SLS;

namespace XGL.Networking {

    public class VersionManager {

        XGLApp app;
        string _name;
        string _ver;
        string _archiveType;
        long _id;
        string link;
        string databaseText;
        string appsDirectory;
        string latestExecutablePath;
        bool _isUpdate;

        public string[] AvaibleProducts { get; private set; }

        //public VersionManager() { }

        public bool GameExist(string name) {
            string appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            string appFile = Path.Combine(appsDirectory, name, name + ".exe");
            return Directory.Exists(appsDirectory) && File.Exists(appFile);
        }

        string GetLatestLink(string name) {
            string version = string.Empty;
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `xgl_products` WHERE `name`=" + '"' + name + '"', Database.Connection);
            try {
                //Fill adapter.
                adapter.SelectCommand = command;
                adapter.Fill(table);
                //Open connection and read everything, what needed.
                Database.OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while(dr.Read()) {
                    databaseText = dr.GetString("latestDownloadLinks");
                    _id = dr.GetInt64("id");
                    _ver = version = databaseText.Split('{')[0];
                    link = databaseText.Split('{')[1];
                }
                //Close reader and connection.
                dr.Close();
                Database.CloseConnection();
                return version;
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void Launch(XGLApp game, string path = "") {
            if(game.Custom) {
                Process.Start(path);
                return;
            }
            //Define variables.
            app = game;
            _name = game.Name;
            appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            latestExecutablePath = Path.Combine(appsDirectory, _name, _name + ".exe");
            //Check for existance of save file.
            if(GameExist(game.Name)) {
                Process p = Process.Start(latestExecutablePath);
                game.ProcessID = p.Id;
                p.EnableRaisingEvents = true;
                p.Exited += (object sender, EventArgs e) => {
                    MainWindow.IsRunningApp = true;
                };
                MainWindow.Instance.gamesControl.UpdatePageOf(game);
                return;
            }
        }

        public void Install(XGLApp game, string archiveType = "zip", bool isUpdate = false) {
            //Define variables.
            _isUpdate = isUpdate;
            app = game;
            _name = game.Name;
            _archiveType = archiveType;
            appsDirectory = Path.Combine(Environment.CurrentDirectory, "apps");
            latestExecutablePath = Path.Combine(appsDirectory, _name, _name + ".exe");
            try {
                //Start downloading.
                GetLatestLink(_name);
                MainWindow.Instance.gamesControl.progress_bar_adv.Visibility = Visibility.Visible;
                MainWindow.Instance.gamesControl.UpdateControls(false);
                if(!RegistrySLS.LoadBool("AdvancedDownloads", false))
                    MainWindow.Instance.gamesControl.downloaderPB_game.Text = _name;
                GDFileDownloader downloader = new GDFileDownloader();
                downloader.DownloadProgressChanged += (object sender, GDFileDownloader.DownloadProgress progress) => {
                    MainWindow.Instance.gamesControl.downloaderPB.Visibility = Visibility.Visible;
                    MainWindow.Instance.gamesControl.downloaderPB.Value = progress.ProgressPercentage;
                    MainWindow.Instance.gamesControl.downloaderPB_perc.Text = progress.ProgressPercentage.ToString() + "%";
                    MainWindow.Instance.gamesControl.downloaderPB_perc.Visibility = Visibility.Visible;
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
                MainWindow.Instance.gamesControl.UpdateControls(true);
                //Extract files from zip.
                ZipFile.ExtractToDirectory(Path.Combine(appsDirectory, $"{_name}.{_archiveType}"), Path.Combine(appsDirectory, _name));
                File.Delete(Path.Combine(appsDirectory, $"{_name}.{_archiveType}"));
                //Add to registred games.
                MainWindow.Instance.gamesControl.WriteToGamesFile(Path.Combine(appsDirectory, _name, _name + ".exe"), false, _name, _ver, _id);
                MainWindow.Instance.gamesControl.Clear();
                MainWindow.Instance.gamesControl.Reload();
                if(!RegistrySLS.LoadBool("AdvancedDownloads", false)) {
                    MainWindow.Instance.gamesControl.progress_bar_adv.Visibility = Visibility.Collapsed;
                    MainWindow.Instance.gamesControl.downloaderPB_game.Text = string.Empty;
                } else MainWindow.Instance.gamesControl.downloaderPB.Visibility = Visibility.Collapsed;
                MainWindow.Instance.gamesControl.downloaderPB.Value = 0;
                MainWindow.Instance.gamesControl.downloaderPB_perc.Visibility = Visibility.Collapsed;
                //Send notification.
                if(!_isUpdate)
                    new ToastContentBuilder()
                        .AddArgument("action", "openLauncher")
                        .AddText(_name + LocalizationManager.I.dictionary["mw.g.down.main"])
                        .AddText(LocalizationManager.I.dictionary["mw.g.dru.splash"])
                        .Show(toast => {
                            toast.ExpirationTime = DateTime.Now.AddMinutes(2.5);
                            toast.Activated += (Windows.UI.Notifications.ToastNotification _sender, object args) => {
                                MainWindow.Instance.Reload();
                                MainWindow.Instance.gamesControl.OpenPageOf(app);
                            };
                        });
                else
                    new ToastContentBuilder()
                    .AddArgument("action", "openLauncher")
                    .AddText(_name + LocalizationManager.I.dictionary["mw.g.upd.main"])
                    .AddText(LocalizationManager.I.dictionary["mw.g.dru.splash"])
                    .Show(toast => {
                        toast.ExpirationTime = DateTime.Now.AddMinutes(2.5);
                        toast.Activated += (Windows.UI.Notifications.ToastNotification _sender, object args) => {
                            MainWindow.Instance.Reload();
                            MainWindow.Instance.gamesControl.OpenPageOf(app);
                        };
                    });
                MainWindow.Instance.gamesControl.downloaderPB.Value = 0;
                MainWindow.Instance.gamesControl.downloaderPB.Visibility = Visibility.Collapsed;
                MainWindow.Instance.gamesControl.Clear();
                MainWindow.Instance.gamesControl.Reload();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                Debug.WriteLine(ex.Message);
            }
        }

    }


}
 