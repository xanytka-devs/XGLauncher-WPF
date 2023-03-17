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

namespace XGL {
    public partial class MainWindow : Window {
        public static MainWindow Instance { get; private set; }
        string path;
        public long installID = 1;
        static RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true);
        public MainWindow() {
            InitializeComponent();
            Instance = this;
            if (!softwareKey.GetSubKeyNames().Contains("XGLauncher")) {
                pageSelectInstall.Visibility = Visibility.Collapsed;
            } else {
                nextWelcome.Content = "Продолжить";
                installTB.Text = softwareKey.OpenSubKey("XGLauncher", RegistryKeyPermissionCheck.ReadWriteSubTree)
                    .GetValue("Path", string.Empty).ToString();
                if (string.IsNullOrEmpty(installTB.Text)) nextInstall.IsEnabled = false;
                else nextInstall.IsEnabled = true;
                if(softwareKey.OpenSubKey("XGLauncher", RegistryKeyPermissionCheck.ReadSubTree).GetSubKeyNames().Contains("DevTools")) 
                    inXGLDT.IsEnabled = false;
                if(softwareKey.OpenSubKey("XGLauncher", RegistryKeyPermissionCheck.ReadSubTree).GetValue("Path", "INS")
                    .ToString() != "INS") inXGL.IsEnabled = false;
            }
        }
        void Next(object sender, RoutedEventArgs e) {
            switch((sender as Button).Name) {
                case "nextWelcome":
                    pageWelcome.Visibility = Visibility.Collapsed;
                    break;
                case "nextEULA":
                    pageEULA.Visibility = Visibility.Collapsed;
                    break;
                case "nextSelectInstall":
                    pageSelectInstall.Visibility = Visibility.Collapsed;
                    if((bool)inXGLDT.IsChecked) {
                        addToAutorun.Visibility = Visibility.Collapsed;
                        addToAutorunT.Visibility = Visibility.Collapsed;
                        createShortcut.Margin = new Thickness(40, 220, 0, 0);
                        createShortcutT.Margin = new Thickness(80, 225, 0, 0);
                        InstallTitle.Text = "XGLauncher DevTools";
                        InstallSubTitle.Margin = new Thickness(360, 30, 0, 0);
                        installTB.Text += "\\DevTools";
                    } else {
                        addToAutorun.Visibility = Visibility.Visible;
                        addToAutorunT.Visibility = Visibility.Visible;
                        createShortcut.Margin = new Thickness(40, 260, 0, 0);
                        createShortcutT.Margin = new Thickness(80, 265, 0, 0);
                        InstallTitle.Text = "XGLauncher";
                        InstallSubTitle.Margin = new Thickness(230, 30, 0, 0);
                        installTB.Text += softwareKey.OpenSubKey("XGLauncher", RegistryKeyPermissionCheck.ReadWriteSubTree)
                            .GetValue("Path", string.Empty).ToString();
                    }
                    path = installTB.Text;
                    break;
                case "nextInstall":
                    pageInstall.Visibility = Visibility.Collapsed;
                    if ((bool)inXGLDT.IsChecked) {
                        DownloadTitle.Text = "XGLauncher DevTools";
                        DownloadSubTitle.Margin = new Thickness(360, 30, 0, 0);
                    } else {
                        DownloadTitle.Text = "XGLauncher";
                        DownloadSubTitle.Margin = new Thickness(230, 30, 0, 0);
                    }
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
                    if (!softwareKey.GetSubKeyNames().Contains("XGLauncher"))
                        pageEULA.Visibility = Visibility.Visible;
                    else pageSelectInstall.Visibility = Visibility.Visible;
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
                if (installID == 1) {
                    if (Utils.IsRootDirectory(fileDialog.FileName) ||
                    a != "XGLauncher")
                        fileDialog.FileName += "XGLauncher";
                }
                if (installID == 2) {
                    if (Utils.IsRootDirectory(fileDialog.FileName) ||
                    a != "XGLauncher") {
                        fileDialog.FileName += "XGLauncher";
                        a += "XGLauncher";
                    }
                    if (a != "DevTools")
                        fileDialog.FileName += "\\DevTools";
                }
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
            try {
                using (WebClient client = new WebClient()) {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    if(installID == 1)
                        client.DownloadFileAsync(new Uri(link), Path.Combine(path, "XGLauncher.zip"));
                    else client.DownloadFileAsync(new Uri(link), Path.Combine(path, "XGLauncher DevTools.zip"));
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
                    if (addToAutorun.IsChecked == true && installID != 2) {
                        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        rkApp.SetValue("XGLauncher", Path.Combine(path, "XGLauncher.exe"));
                    }
                    if (startAfterInstall.IsChecked == true || createShortcut.IsChecked == true) {
                        Shortcut sc = new Shortcut() {
                            Path = Path.Combine(path, "XGLauncher.exe"),
                            Description = "XGLauncher",
                            WorkingDirectory = path
                        };
                        if(installID == 2)
                            sc = new Shortcut() {
                                Path = Path.Combine(path, "XGLauncher DevTools.exe"),
                                Description = "XGLauncher DevTools",
                                WorkingDirectory = path
                            };
                        string sh = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk";
                        if (installID == 2) sh = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + 
                                "XGLauncher DevTools.lnk";
                        sc.Save(sh);
                        if (createShortcut.IsChecked == false) File.SetAttributes(sh, FileAttributes.Hidden);
                    }
                    pageDownload.Visibility = Visibility.Collapsed;
                    if ((bool)inXGLDT.IsChecked) CompleteTitle.Text = "XGLauncher DevTools";
                    else CompleteTitle.Text = "XGLauncher";
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

        void InstallSelectApp(object sender, RoutedEventArgs e) {
            inXGL.IsChecked = false;
            inXGLDT.IsChecked = false;
            switch ((sender as ToggleButton).Name) {
                case "inXGL":
                    inXGL.IsChecked = true;
                    installID = 1;
                    break;
                case "inXGLDT":
                    inXGLDT.IsChecked = true;
                    installID = 2;
                    break;
            }
            nextSelectInstall.IsEnabled = true;
        }
    }
}
