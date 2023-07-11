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
using System.Windows.Media.Animation;
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
            Storyboard storyboard = new Storyboard();
            Duration duration = new Duration(TimeSpan.FromMilliseconds(250));
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            DoubleAnimation animation = new DoubleAnimation {
                EasingFunction = ease,
                Duration = duration
            };
            animation.Completed += (object _sender, EventArgs _e) => {
                MainWindow.Instance.ChangelogPg.Visibility = Visibility.Collapsed;
                RegistrySLS.Save("ShownChangelog", App.CurrentVersion);
            };
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, MainWindow.Instance.ChangelogPg);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.Opacity)"));
            animation.From = 1;
            animation.To = 0;
            storyboard.Begin();
        }

    }
}
