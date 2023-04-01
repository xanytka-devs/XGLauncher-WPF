using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using XGL;
using XGL.Networking.Database;
using XGL.SLS;

namespace XGLauncher.Dialogs {

    /// <summary>
    /// Логика взаимодействия для TempURLDialog.xaml
    /// </summary>
    
    public partial class TempURLDialog : Window {

        public TempURLDialog() {
            InitializeComponent();
        }

        void PublishImage(object sender, RoutedEventArgs e) {
            if(string.IsNullOrEmpty(imageURL.Text) || !imageURL.Text.Contains("http")) {
                imageURLT.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            int i = 1;
            if (Database.GetValue(App.CurrentAccount, "icon").ToString().Split('{')[0] != "default")
                i = int.Parse(Database.GetValue(App.CurrentAccount, "icon").ToString().Split('{')[0].Split('_')[2]) + 1;
            Database.SetValue(Database.DBDataType.DT_ICON, $"{RegistrySLS.LoadString("LastID").Split(';')[1]}_profile_{i}" + "{" + imageURL.Text);
            Close();
            MainWindow.Instance.Reload();
        }

    }

}
