using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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
using XGL.Networking;
using XGL.SLS;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для News.xaml
    /// </summary>

    public partial class News : UserControl {

        readonly List<StackPanel> panels = new List<StackPanel>();
        readonly List<Button> newsBtns = new List<Button>();
        List<NewsBase> news = new List<NewsBase>();
        readonly List<Image> newsBtnImages = new List<Image>();
        internal int tabClicks = 2;

        public News() {
            InitializeComponent();
            panels.Add(pan0);
            panels.Add(pan1);
            panels.Add(pan2);
            panels.Add(pan3);
            panels.Add(pan4);
        }

        public void Reload() {
            if(RegistrySLS.LoadBool("DClickToReloadTab", false)) {
                if(tabClicks != 2) return;
                Clear();
                tabClicks = 0;
            }
            //Get data from database.
            news = Database.GetNewsItems(Database.GetAppIDs());
            //Generate news buttons.
            GenerateNewsButtons();
        }

        public void Clear() {
            if(RegistrySLS.LoadBool("DClickToReloadTab", false))
                if(tabClicks != 2) return;
            for (int i = 0; i < panels.Count; i++)
                panels[i].Children.Clear();
            newsBtns.Clear();
            newsBtnImages.Clear();
            news.Clear();
            BackToMainPage(null, null);
        }

        public void BackToMainPage(object sender, RoutedEventArgs e) {
            newsPost.Visibility = Visibility.Collapsed;
            BGImage.Source = null;
        }

        void GenerateNewsButtons() {
            int curNP = 0;

            for (int i = 0; i < news.Count; i++) {
                //INNER//
                //Border for text visibility.
                Border visArea = new Border() {
                    Background = new LinearGradientBrush(Color.FromRgb(0, 0, 0), Color.FromArgb(0, 0, 0, 0),
                        new Point(0.5, 1), new Point(0.5, 0))
                };
                //Text block with news name.
                TextBlock tbName = new TextBlock() {
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(5),
                    Text = news[i].Name
                };
                visArea.Child = tbName;

                //BACKGROUND//
                //Border
                Image img = new Image() {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Stretch = Stretch.UniformToFill
                };
                newsBtnImages.Add(img);

                //BODY//
                //Grid.
                Grid mainGrid = new Grid();
                mainGrid.Children.Add(img);
                mainGrid.Children.Add(visArea);
                //Display box.
                Button mainBorder = new Button {
                    Width = 200,
                    Height = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    Content = mainGrid,
                    Style = (Style)FindResource("NewsButton")
                };
                newsBtns.Add(mainBorder);
                mainBorder.Click += (s, e) => {
                    int ind = newsBtns.IndexOf(s as Button);
                    AppNameTB.Text = news[ind].Name;
                    BodyText.Text = news[ind].Body;
                    LoadImage(ind);
                    newsPost.Visibility = Visibility.Visible;
                };

                //Add to stackpanel.
                panels[curNP].Children.Add(mainBorder);

                //Change column.
                curNP++;
                if(curNP == 5) curNP = 0;
            }
            Scroll_Changed(ScrollBar, null);
        }

        public void ApplyLocalization() {

            //LocalizationManager l = LocalizationManager.I;

        }

        void ExitNewsPost(object sender, RoutedEventArgs e) { 
            newsPost.Visibility = Visibility.Collapsed;
            BGImage.Source = null;
        }

        private void Scroll_Changed(object sender, ScrollChangedEventArgs e) {
            for (int i = 0; i < newsBtns.Count; i++) {
                if(Utils.I.IsControlVisible(newsBtns[i], newsBtns[i].Parent as FrameworkElement)) {
                    LoadImageBtn(i, newsBtnImages[i]);
                }
            }
        }

        async void LoadImage(int i) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", news[i].Icon.Split('{')[0] + ".png");
            if(!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(news[i].Icon.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            BGImage.Source = logo;
        }

        async void LoadImageBtn(int i, Image img) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", news[i].MainIcon.Split('{')[0] + ".jpg");
            if(!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(news[i].MainIcon.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage {
                CacheOption = BitmapCacheOption.OnLoad
            };
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            img.Source = logo;
        }

    }

}
