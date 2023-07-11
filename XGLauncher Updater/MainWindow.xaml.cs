using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;

namespace XGL.Update {
    public partial class MainWindow : Window {
        string path;
        string link;
        static RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true);
        public MainWindow() { 
            InitializeComponent();
            softwareKey = softwareKey.OpenSubKey("Xanytka Software");
            path = softwareKey.GetValue("Path").ToString();
            if(App.UpdateIteration == 0) {
                pageWelcome.Visibility = Visibility.Visible;
                link = Database.GetValue();
                if(softwareKey.GetValue("Version").ToString() == link.Split('{')[0]) pageNoUpdates.Visibility = Visibility.Visible;
            } else if(App.UpdateIteration == 1) ContinueUpdate();
            else if(App.UpdateIteration == 2) EndUpdate();
        }
        void Update(object sender, RoutedEventArgs e) {
            pageWelcome.Visibility = Visibility.Collapsed;
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles()) {
                if(file.Name != "XGLauncher Updater.exe" & 
                    file.Name != "XGLauncher Updater.exe.config" & 
                    file.Name != "XGLauncher Updater.pdb" &
                    file.Name != "MySql.Data.dll" &
                    file.Name != "WpfAnimatedGif.dll") file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories()) {
                if(dir.Name != "apps" 
                    && dir.Name != "cache" 
                    && dir.Name != "logs") dir.Delete(true);
            }
            StartInstallation();
        }
        void StartInstallation() {
            try {
                using (WebClient client = new WebClient()) {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    Directory.CreateDirectory(Path.Combine(path, "temp"));
                    client.DownloadFileAsync(new Uri(link.Split('{')[1]), Path.Combine(path, "temp", "XGLauncher.zip"));
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        void ContinueUpdate() {
            Show();
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles()) file.Delete();
            File.Copy(Path.Combine(path, "temp", "XGLauncher.zip"), Path.Combine(path, "XGLauncher.zip"));
            File.Copy(Path.Combine(path, "temp", "update.config"), Path.Combine(path, "update.config"));
            ZipFile.ExtractToDirectory(Path.Combine(path, "XGLauncher.zip"), path);
            File.Delete(Path.Combine(path, "XGLauncher.zip"));
            ProcessStartInfo updater = new ProcessStartInfo() {
                FileName = Path.Combine(path, "XGLauncher Updater.exe"),
                Arguments = "/new"
            };
            Process.Start(updater);
            Close();
        }
        void EndUpdate() {
            Show();
            DirectoryInfo di = new DirectoryInfo(Path.Combine(path, "temp"));
            foreach (FileInfo file in di.GetFiles()) file.Delete();
            Directory.Delete(Path.Combine(path, "temp"));
            string[] configs = File.ReadAllText(Path.Combine(path, "update.config")).Split('\n');
            if(bool.Parse(configs[0]) || bool.Parse(configs[1])) {
                Shortcut sc = new Shortcut() {
                    Path = Path.Combine(path, "XGLauncher.exe"),
                    Description = "XGLauncher",
                    WorkingDirectory = path
                };
                string sh = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "XGLauncher.lnk");
                sc.Save(sh);
                try { if(!bool.Parse(configs[1])) File.SetAttributes(sh, FileAttributes.Hidden); else File.SetAttributes(sh, FileAttributes.Normal); }
                catch (Exception ex) {
                    if(bool.Parse(configs[0])) Process.Start(sh);
                    Thread.Sleep(100);
                    File.Delete(Path.Combine(path, "update.config"));
                    if(!bool.Parse(configs[1])) File.Delete(sh);
                    Debug.WriteLine(ex.Message);
                    Close();
                }
            }
            Close();
        }

        void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {

            if(e.Error != null) {
                MessageBox.Show($"Error: {e.Error.Message}", "XGLauncher Setup");
            } else if(!e.Cancelled) {
                try {
                    ZipFile.ExtractToDirectory(Path.Combine(path, "temp", "XGLauncher.zip"), Path.Combine(path, "temp"));
                    ProcessStartInfo updater = new ProcessStartInfo() {
                        FileName = Path.Combine(path, "temp", "XGLauncher Updater.exe"),
                        Arguments = "/old"
                    };
                    File.WriteAllText(Path.Combine(path, "temp", "update.config"), startAfterInstall.IsChecked.ToString() + "\n" + createShortcut.IsChecked.ToString());
                    Process.Start(updater);
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
