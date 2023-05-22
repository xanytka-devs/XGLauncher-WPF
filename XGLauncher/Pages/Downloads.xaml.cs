using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Downloads.xaml
    /// </summary>
    
    public partial class Downloads : UserControl {

        class DownloadProfile {

            public DownloadProfile() {

            }

        }

        readonly List<DownloadProfile> downloads = new List<DownloadProfile>();

        public Downloads() {
            InitializeComponent();
        }

        public void Clear() {
            


        }

        public void Reload() {
            


        }

        void ReadDownloadsFile() {
            string appsFile = Path.Combine(App.AppsFolder, "Downloads");
            if(File.Exists(appsFile)) {
                string[] applications = File.ReadAllText(appsFile).Split('\n');
                /*for (int i = 0; i < applications.Length; i++) {
                    if(!string.IsNullOrEmpty(applications[i])) {
                        downloads.Add(new DownloadProfile(long.Parse(applications[i].Split('^')[0]), applications[i].Split('^')[1],
                                applications[i].Split('^')[4], applications[i].Split('^')[3]));
                    }
                }*/
            } else {
                File.Create(appsFile);
                File.SetAttributes(appsFile, FileAttributes.Hidden);
            }
        }

        void WriteToDownloadsFile() {
            /*string appsFile = Path.Combine(App.AppsFolder, "Downloads");
            if(string.IsNullOrEmpty(name)) name = path.Split('\\')[path.Split('\\').Length - 1];
            string add = $"{id}^{name}^{version}^{path}\n";
            try {
                File.WriteAllText(appsFile, File.ReadAllText(appsFile) + add);
            }
            catch {
                File.WriteAllText(appsFile, add);
            }*/
        }

        void DeleteFromDownloadsFile(long id) {
            string appsFile = Path.Combine(App.AppsFolder, "Downloads");
            if(File.Exists(appsFile)) {
                string[] applications = File.ReadAllText(appsFile).Split('\n');
                string[] newList = new string[applications.Length - 1];
                for (int i = 0; i < applications.Length; i++) {
                    if(!string.IsNullOrEmpty(applications[i])) {
                        if(applications[i].Split('^')[0] != id.ToString())
                            newList[i] = applications[i];
                    }
                }
            }
        }

    }

}
