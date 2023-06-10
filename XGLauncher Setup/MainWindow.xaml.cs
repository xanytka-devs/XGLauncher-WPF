using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace XGLS {
    public partial class MainWindow : Window {
        public static MainWindow Instance { get; private set; }
        string path;
        public MainWindow() {
            InitializeComponent();
            Instance = this;
        }
        void Next(object sender, RoutedEventArgs e) {
            switch((sender as Button).Name) {
                case "nextWelcome":
                    pageWelcome.Visibility = Visibility.Collapsed;
                    break;
                case "nextEULA":
                    pageEULA.Visibility = Visibility.Collapsed;
                    break;
                case "nextInstall":
                    pageInstall.Visibility = Visibility.Collapsed;
                    StartInstallation();
                    break;
                case "nextComplete":
                    if (startAfterInstall.IsChecked == true)
                        Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk");
                    Close();
                    if (createShortcut.IsChecked == false) 
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk");
                    break;
            }
        }

        void Back(object sender, RoutedEventArgs e) {
            switch ((sender as Button).Name) {
                case "backEULA":
                    pageWelcome.Visibility = Visibility.Visible;
                    break;
                case "backSelectInstall":
                    pageEULA.Visibility = Visibility.Visible;
                    break;
                case "backInstall":
                    pageEULA.Visibility = Visibility.Visible;
                    break;
            }
        }
        void InstallSelectPath(object sender, RoutedEventArgs e) {
            SaveFileDialog fileDialog = new SaveFileDialog() {
                FileName = "Лаунчер будет установлен сюда",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            };
            if(!string.IsNullOrEmpty(path)) fileDialog.InitialDirectory = path;
            if (fileDialog.ShowDialog() == true) {
                fileDialog.FileName = Utils.StringWithoutValueBySplit(fileDialog.FileName, '\\', fileDialog.FileName.Split('\\').Length - 1, true);
                string a = fileDialog.FileName.Split('\\')[fileDialog.FileName.Split('\\').Length - 2];
                if (Utils.IsRootDirectory(fileDialog.FileName) ||
                    a != "XGLauncher")
                    fileDialog.FileName += "XGLauncher";
                if (Directory.Exists(fileDialog.FileName)) {
                    if (!Utils.IsDirectoryEmpty(fileDialog.FileName)) {
                        MessageBox.Show("Selected folder isn't empty.",
                            "XGLauncher", MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        InstallSelectPath(sender, e);
                        return;
                    }
                }
                installTB.Text = path = fileDialog.FileName;
            }
        }
        void StartInstallation() {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string link = Database.GetValue().Split('{')[1];
            if (!Database.OpenConnection() && !Database.CloseConnection()) {
                link = "https://xanytka.ru/downloads/XGLauncher.zip";
            }
            try {
                using (WebClient client = new WebClient()) {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(link), Path.Combine(path, "XGLauncher.zip"));
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
                StartInstallation();
            }

        }

        void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            if(e.Error != null) {
                MessageBox.Show($"Error: {e.Error.Message}", "XGLauncher Setup");
            } else if (!e.Cancelled) {
                try {
                    ZipFile.ExtractToDirectory(Path.Combine(path, "XGLauncher.zip"), path);
                    File.Delete(Path.Combine(path, "XGLauncher.zip"));
                    if (addToAutorun.IsChecked == true) {
                        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        rkApp.SetValue("XGLauncher", Path.Combine(path, "XGLauncher.exe"));
                    }
                    if (startAfterInstall.IsChecked == true || createShortcut.IsChecked == true) {
                        Shortcut sc = new Shortcut() {
                            Path = Path.Combine(path, "XGLauncher.exe"),
                            Description = "XGLauncher",
                            WorkingDirectory = path
                        };
                        string sh = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk";
                        sc.Save(sh);
                        if (createShortcut.IsChecked == false) File.SetAttributes(sh, FileAttributes.Hidden);
                    }
                    pageDownload.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }

        }

        void InstallTB_TextChanged(object sender, TextChangedEventArgs e) {
            if(string.IsNullOrEmpty(installTB.Text)) nextInstall.IsEnabled = false;
            else nextInstall.IsEnabled = true;
        }

    }
}
