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
    /// Логика взаимодействия для WarningBox.xaml
    /// </summary>
    
    public enum ButtonLayout {
        OK,
        OKCancel,
        YesNoCancel,
        YesNo
    }
    public enum DefaultedButton {
        Ok,
        Cancel,
        Yes,
        No
    }
    public enum WBResult {
        Null,
        Ok,
        Cancel,
        Yes,
        No
    }

    public partial class WarningBox : Window {

        public WBResult Result { get; private set; } = WBResult.Null;

        public WarningBox() {
            InitializeComponent();
            Title = "Warning";
            warnText.Text = "Templated text";
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
            Show();
        }
        public WarningBox(string text) {
            InitializeComponent();
            Title = "Warning";
            warnText.Text = text;
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
            Show();
        }
        public WarningBox(string title, string text) {
            InitializeComponent();
            Title = title;
            warnText.Text = text;
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
            Show();
        }
        public WarningBox(string title, string text, ButtonLayout layout) {
            InitializeComponent();
            Title = title;
            warnText.Text = text;
            AnylizeButtonLayout(layout);
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
            Show();
        }
        public WarningBox(string title, string text, ButtonLayout layout, DefaultedButton defaulted) {
            InitializeComponent();
            Title = title;
            warnText.Text = text;
            AnylizeButtonLayout(layout);
            AnylizeDefaultedButton(defaulted);
            MainWindow.Instance.AllWindows.Add(this);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) => MainWindow.Instance.AllWindows.Remove(this);
            Show();
        }

        void ReturnResult(object sender, RoutedEventArgs e) {
            if (sender == ok) Result = WBResult.Ok;
            else if (sender == cancel) Result = WBResult.Cancel;
            else if (sender == yes) Result = WBResult.Yes;
            else if (sender == no) Result = WBResult.No;
        }
        void AnylizeButtonLayout(ButtonLayout layout) {
            switch (layout) {
                case ButtonLayout.OK:
                    ok.Visibility = Visibility.Visible;
                    break;
                case ButtonLayout.OKCancel:
                    ok.Visibility = Visibility.Visible;
                    cancel.Visibility = Visibility.Visible;
                    break;
                case ButtonLayout.YesNoCancel:
                    yes.Visibility = Visibility.Visible;
                    no.Visibility = Visibility.Visible;
                    cancel.Visibility = Visibility.Visible;
                    break;
                case ButtonLayout.YesNo:
                    yes.Visibility = Visibility.Visible;
                    no.Visibility = Visibility.Visible;
                    break;
                default:
                    ok.Visibility = Visibility.Visible;
                    break;
            }
        }
        void AnylizeDefaultedButton(DefaultedButton defaulted) {
            switch (defaulted) {
                case DefaultedButton.Ok:
                    ok.IsDefault = true;
                    break;
                case DefaultedButton.Cancel:
                    cancel.IsDefault = true;
                    break;
                case DefaultedButton.Yes:
                    yes.IsDefault = true;
                    break;
                case DefaultedButton.No:
                    no.IsDefault = true;
                    break;
                default:
                    break;
            }
        }

    }

}
