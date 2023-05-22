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
using XGL.Networking;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Friends.xaml
    /// </summary>
    
    public partial class Friends : UserControl {

        public List<Account> friends = new List<Account>();

        public Friends() {

            InitializeComponent();

        }

    }

}
