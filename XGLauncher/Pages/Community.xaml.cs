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

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Community.xaml
    /// </summary>
    
    public partial class Community : UserControl {

        public Community() {
            InitializeComponent();
        }

        public void ApplyLocalization(string localization) {

            switch (localization) {
                case "ru-RU":

                    break;
                case "en-US":

                    break;
                case "es":

                    break;
            }

        }
    }
}
