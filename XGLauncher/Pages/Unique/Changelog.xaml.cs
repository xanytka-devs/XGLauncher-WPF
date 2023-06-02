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
using System.Windows.Navigation;
using System.Windows.Shapes;
using XGL.SLS;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Changelog.xaml
    /// </summary>
    
    public partial class Changelog : UserControl {

        public Changelog() {
            InitializeComponent();
        }

        void ExitChangelog(object sender, RoutedEventArgs e) {
            MainWindow.Instance.ChangelogPg.Visibility = Visibility.Collapsed;
            RegistrySLS.Save("ShownChangelog", App.CurrentVersion);
        }

    }
}
