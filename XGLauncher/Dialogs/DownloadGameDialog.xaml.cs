using System;
using System.Windows;
using System.Windows.Controls;

namespace XGL.Dialogs {

    /// <summary>
    /// Логика взаимодействия для DownloadGameDialog.xaml
    /// </summary>

    public partial class DownloadGameDialog : Window {

        public string AppPath { get; private set; }
        public bool CreateShotcut_Desktop { get; private set; }
        public bool CreateShotcut_StartMenu { get; private set; }

        public DownloadGameDialog(string appName) {
            InitializeComponent();
            Title = "Download " + appName;
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
        }

        protected override void OnClosed(EventArgs e) {
            ReturnResult(r_c, null);
            MainWindow.Instance.AllWindows.Remove(this);
            base.OnClosed(e);
        }

        void SelectPath(object sender, RoutedEventArgs e) {


        }

        void ReturnResult(object sender, RoutedEventArgs e) {

            AppPath = pathTB.Text;
            CreateShotcut_Desktop = (bool)cas_Desktop.IsChecked;
            CreateShotcut_StartMenu = (bool)cas_StartMenu.IsChecked;

            switch ((sender as Button).Name) {
                case "r_i":
                    DialogResult = true;
                    break;
                case "r_c":
                    DialogResult = false;
                    break;
            }

        }

    }
}
