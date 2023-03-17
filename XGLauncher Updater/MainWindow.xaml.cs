using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace XGL.Update {
    public partial class MainWindow : Window {
        string path;
        string link;
        static RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true);
        public MainWindow() { 
            InitializeComponent();
            softwareKey = softwareKey.OpenSubKey("XGLauncher");
            link = Database.GetValue();
            path = softwareKey.GetValue("Path").ToString();
            if(softwareKey.GetValue("Version").ToString() == link.Split('{')[0]) pageNoUpdates.Visibility = Visibility.Visible;
        }
        void Update(object sender, RoutedEventArgs e) {
            pageWelcome.Visibility = Visibility.Collapsed;
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles()) {
                if (file.Name != "XGLauncher Updater.exe" & 
                    file.Name != "XGLauncher Updater.exe.config" & 
                    file.Name != "XGLauncher Updater.pdb") file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
            StartInstallation();
        }
        void StartInstallation() {
            try {
                using (WebClient client = new WebClient()) {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(link), Path.Combine(path, "XGLauncher.zip"));
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {

            if (e.Error != null) {
                MessageBox.Show($"Error: {e.Error.Message}", "XGLauncher Setup");
            } else if (!e.Cancelled) {
                try {
                    ZipFile.ExtractToDirectory(Path.Combine(path, "XGLauncher.zip"), path);
                    File.Delete(Path.Combine(path, "XGLauncher.zip"));
                    if (startAfterInstall.IsChecked == true || createShortcut.IsChecked == true) {
                        Shortcut sc = new Shortcut() {
                            Path = Path.Combine(path, "XGLauncher.exe"),
                            Description = "XGLauncher",
                            WorkingDirectory = path
                        };
                        string sh = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk";
                        sc.Save(sh);
                        if (createShortcut.IsChecked == false) File.SetAttributes(sh, FileAttributes.Hidden);
                        else File.SetAttributes(sh, FileAttributes.Normal);
                        if (startAfterInstall.IsChecked == true)
                            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk");
                        if (createShortcut.IsChecked == false) File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk");
                    }
                    Close();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }

        }
        private void Exit(object sender, RoutedEventArgs e) => Close();
    }
}
