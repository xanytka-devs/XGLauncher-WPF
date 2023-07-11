using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
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

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Publisher.xaml
    /// </summary>

    public partial class Publisher : UserControl {

        public string PublisherName { get; set; }
        string PublisherImgURL;
        bool isVerified;
        List<long> featured = new List<long>();
        readonly ImageSource defImgPath;
        readonly List<Button> prodBtns = new List<Button>();
        readonly List<Image> prodBtnImages = new List<Image>();
        readonly List<Border> contents = new List<Border>();
        public readonly List<StoreBase> prodPages = new List<StoreBase>();

        public Publisher() {

            InitializeComponent();
            defImgPath = pubImg.Source;

        }

        public void Clear() {

            verified.Visibility = Visibility.Collapsed;
            pubName.Text = string.Empty;
            prodPages.Clear();
            featured.Clear();
            prodBtns.Clear();
            prodBtnImages.Clear();
            allProducts.Children.Clear();
            featuredProducts.Children.Clear();
            scroller.ScrollToTop();
            contents.Clear();
            tabs.SelectedIndex = 0;

        }

        public void Reload() {

            string pubI = Database.GetPublisher(PublisherName);
            if(string.IsNullOrEmpty(pubI)) return;
            string[] pubInfo = pubI.Split('\\');
            PublisherImgURL = pubInfo[0];
            isVerified = Utils.I.BoolFromInt(int.Parse(pubInfo[1]));
            if (pubInfo[2].Split(';').Length > 0)
                for (int i = 0; i < pubInfo[2].Split(';').Length; i++) {
                    if (!string.IsNullOrEmpty(pubInfo[2].Split(';')[i]))
                        featured.Add(long.Parse(pubInfo[2].Split(';')[i]));
                }
            else featuredTab.Visibility = Visibility.Collapsed;

            LocalizationManager l = LocalizationManager.I;
            pubName.Text = PublisherName;
            verified.ToolTip = l.dictionary["mw.s.ver"];
            if(isVerified) verified.Visibility = Visibility.Visible;
            LoadImage();
            GenerateButtons();
            Scroll_Changed(scroller, null);

        }

        void BackToMainPage(object sender, RoutedEventArgs e) => MainWindow.Instance.CollapseAllPages(MainWindow.Instance.StorePg);

        async void LoadImage() {
            if(PublisherImgURL.Split('{')[1] == "INS") { pubImg.Source = defImgPath; return; }
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", PublisherImgURL.Split('{')[0] + ".jpg");
            if(!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(PublisherImgURL.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            pubImg.Source = logo;
        }

        void GenerateButtons() {
            foreach (StoreBase page in MainWindow.Instance.storeControl.storePages)
                if(page.Publisher == PublisherName) prodPages.Add(page);
            for(int i = 0; i < prodPages.Count; i++) {
                Button main = CreateFromTemplate(prodPages[i]);
                //Add to column.
                allProducts.Children.Add(main);
                prodBtns.Add(main);
                //Add to featured.
                if(featured.Count > 0) {
                    featuredTab.Visibility = Visibility.Visible;
                    for(int f = 0; f < featured.Count; f++) {
                        if(prodPages[i].ID == featured[f]) {
                            Button fb = new Button() {
                                Name = main.Name + "_featured",
                                Style = main.Style,
                                Height = 80,
                                Margin = new Thickness(0, 0, 0, 5),
                                Content = contents[i]
                            };
                            fb.Click += StoreProductButton_Click;
                            featuredProducts.Children.Add(fb);
                            break;
                        }
                    }
                    prodBtns[i].Content = contents[i];
                } else featuredTab.Visibility = Visibility.Collapsed;
            }
        }

        Button CreateFromTemplate(StoreBase page) {
            LocalizationManager l = LocalizationManager.I;
            //INNER//
            //Preview image.
            Image preview = new Image {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.UniformToFill
            };
            prodBtnImages.Add(preview);
            //Product name.
            TextBlock nameTB = new TextBlock {
                FontWeight = FontWeights.Bold,
                FontSize = 36,
                VerticalAlignment = VerticalAlignment.Center,
                Text = page.Name,
                Margin = new Thickness(5, 0, 0, 0)
            };
            //Product price.
            TextBlock priceTB = new TextBlock {
                FontSize = 24,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = page.Price
            };
            string cost = page.Price.ToString().ToLower();
            if (cost == "tbd")
                priceTB.Text = l.dictionary["mw.s.tbd"];
            else if (cost == "free") priceTB.Text = l.dictionary["mw.s.free"];
            else priceTB.Text = page.Price.ToString();

            //Holder.
            Grid inner = new Grid() {
                ColumnDefinitions = {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            inner.Children.Add(preview);
            inner.Children.Add(nameTB);
            inner.Children.Add(priceTB);
            preview.SetValue(Grid.ColumnProperty, 0);
            nameTB.SetValue(Grid.ColumnProperty, 1);
            priceTB.SetValue(Grid.ColumnProperty, 2);
            //OUTER//
            Border backHolder = new Border {
                Background = new SolidColorBrush(Color.FromRgb(57, 57, 57)),
                CornerRadius = new CornerRadius(5),
                Child = inner
            };
            //Button body.
            Regex sWhitespace = new Regex(@"\s+");
            Button main = new Button {
                Style = (Style)FindResource("NewsButton"),
                Height = 80,
                Margin = new Thickness(0, 0, 0, 5),
                Content = backHolder,
                Name = sWhitespace.Replace((page.Name + page.ID).Replace("'",
                string.Empty), string.Empty)
            };
            contents.Add((Border)main.Content);
            main.Click += StoreProductButton_Click;
            return main;
        }

        void StoreProductButton_Click(object sender, RoutedEventArgs e) {
            try {
                int ind = prodBtns.IndexOf(sender as Button);
                MainWindow.Instance.storeControl.OpenPageOf(prodPages[ind].ID);
            }
            catch {
                int ind = featuredProducts.Children.IndexOf(sender as Button);
                MainWindow.Instance.storeControl.OpenPageOf(featured[ind]);
            }
        }

        void Scroll_Changed(object sender, ScrollChangedEventArgs e) {
            for (int i = 0; i < prodBtns.Count; i++)
                if(Utils.I.IsControlVisible(prodBtns[i], allProducts))
                    LoadImage(prodBtnImages[i], prodPages[i].StoreBanner);
        }

        async void LoadImage(Image img, string URL) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if(!File.Exists(nameOfImg))
                await Utils.I.DownloadFileAsync(URL.Split('{')[1], nameOfImg);
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
