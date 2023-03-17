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

namespace XGL.Dialogs {

    /// <summary>
    /// Логика взаимодействия для TempDialog.xaml
    /// </summary>
    public partial class TempDialog : Window {

        public TempDialog() {
            InitializeComponent();
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
        }

        public void Log(string msg) => Output.Text += msg + "\n";

    }
}
