using System.Windows;
using System.Windows.Controls;

namespace XGL.Dialogs {

    /// <summary>
    /// Логика взаимодействия для ErrorBox.xaml
    /// </summary>

    public partial class ErrorBox : Window {

        public enum ErrorBoxEType {
            Network,
            Protection,
            Plugin,
            Storage,
            ForceLoad,
        }

        public class ErrorBoxButton {
            public string Content { get; private set; }
            public bool IsDefault { get; private set; }
            public bool IsCancel { get; private set; }
            public bool IsVisible { get; private set; }
            public RoutedEventHandler ButtonFunction { get; private set; }
            public ErrorBoxButton(string content, bool isDefault, bool isCancel, RoutedEventHandler function, bool isVisible = false) {
                Content = content;
                IsDefault = isDefault;
                IsCancel = isCancel;
                RoutedEventHandler buttonFunction = function;
                IsVisible = isVisible;
            }
        }

        string _ErrorTitle = string.Empty;
        public string ErrorTitle { get { return _ErrorTitle; } set { errorTitle.Text = value; _ErrorTitle = value; } }
        string _ErrorDescription = string.Empty;
        public string ErrorDescription { get { return _ErrorDescription; } set { errorDescription.Text = value; _ErrorDescription = value; } }
        ErrorBoxEType _ErrorImg = ErrorBoxEType.ForceLoad;
        public ErrorBoxEType ErrorImage { get { return _ErrorImg; } set { SetImg(value); _ErrorImg = value; } }
        ErrorBoxButton _ErrorBtn1 = new ErrorBoxButton("1", false, false, (object sender, RoutedEventArgs e) => { return; }, true);
        public ErrorBoxButton ErrorButton1 { get { return _ErrorBtn1; } set { InstanceBtn(btn1, value); _ErrorBtn1 = value; } }
        ErrorBoxButton _ErrorBtn2 = new ErrorBoxButton("2", false, false, (object sender, RoutedEventArgs e) => { return; }, true);
        public ErrorBoxButton ErrorButton2 { get { return _ErrorBtn2; } set { InstanceBtn(btn2, value); _ErrorBtn2 = value; } }

        public ErrorBox() {
            InitializeComponent();
        }

        void SetImg(ErrorBoxEType type) {
            switch (type) {
                case ErrorBoxEType.Network:
                    noWiFiError.Visibility = Visibility.Visible;
                    break;
                case ErrorBoxEType.Protection:
                    protectionError.Visibility = Visibility.Visible;
                    break;
                case ErrorBoxEType.Plugin:
                    pluginError.Visibility = Visibility.Visible;
                    break;
                case ErrorBoxEType.Storage:
                    storageError.Visibility = Visibility.Visible;
                    break;
                case ErrorBoxEType.ForceLoad:
                    forceLoadError.Visibility = Visibility.Visible;
                    break;
            }
        }

        void InstanceBtn(Button btn, ErrorBoxButton val) { 
            btn.Visibility = val.IsVisible ? Visibility.Collapsed : Visibility.Visible; 
            btn.Content = val.Content;
            btn.IsDefault = val.IsDefault;
            btn.Click += val.ButtonFunction;
            btn.IsCancel = val.IsCancel;
        }
    }

}
