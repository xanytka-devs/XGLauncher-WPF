using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using System.IO;
using XGL.SLS;
using XGL.Networking;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Store.xaml
    /// </summary>

    public partial class Store : UserControl {

        readonly List<Button> storeBtns = new List<Button>();
        readonly List<Image> storeBtnImages = new List<Image>();
        public List<StoreBase> storePages = new List<StoreBase>();
        StoreBase currentApp;
        long currentIndex;
        internal int tabClicks = 2;

        public class ShopButton {
            public Button Preview;
            public Page BodyPage;
        }

        public Store() {
            InitializeComponent();
        }

        public void Reload() {
            if (RegistrySLS.LoadBool("DClickToReloadTab", false)) {
                if (tabClicks != 2) return;
                Clear();
                tabClicks = 0;
            }
            LoadContent();
            GenerateButtons();
            if (RegistrySLS.LoadBool("AutoStoreSearch", false)) {
                searchBtn.Visibility = Visibility.Collapsed;
                searchBar.Margin = new Thickness(0, 0, 0, 0);
                searchBarPlaceholder.Margin = new Thickness(10, 0, 0, 0);
            } else {
                searchBtn.Visibility = Visibility.Visible;
                searchBar.Margin = new Thickness(40, 0, 0, 0);
                searchBarPlaceholder.Margin = new Thickness(50, 0, 0, 0);
            }
            Scroll_Changed(ScrollViwerMain, null);
        }

        public void Clear() {
            if (RegistrySLS.LoadBool("DClickToReloadTab", false))
                if (tabClicks != 2) return;
            storePages.Clear();
            storeBtns.Clear();
            storeBtnImages.Clear();
            ShopBelt.Children.Clear();
            searchBar.Text = string.Empty;
            ScrollViwerMain.ScrollToTop();
            BackToMainPage(null, null);
        }

        public void ApplyLocalization() {
            LocalizationManager l = LocalizationManager.I;
            searchBarPlaceholder.Text = l.dictionary["mw.search"];
            DescriptionTop.Text = l.dictionary["gn.desc"];
            Publisher.Text = l.dictionary["mw.s.pub"];
        }

        void LoadContent() {
            storePages = Database.GetStoreItems();
            CheckItems();
        }

        int cii = 0;
        void CheckItems() {
            if (cii == storePages.Count) return;
            if (storePages[cii].Avaibility == 2) storePages.Remove(storePages[cii]);
            else cii++;
            CheckItems();
        }

        void GenerateButtons() {
            LocalizationManager l = LocalizationManager.I;
            for (int i = 0; i < storePages.Count; i++) {
                //INNER//
                //Preview image.
                Image preview = new Image {
                    HorizontalAlignment = Utils.I.CharToHA(storePages[i].StoreBannerAligment.ToCharArray()[0]),
                    VerticalAlignment = Utils.I.CharToVA(storePages[i].StoreBannerAligment.ToCharArray()[1]),
                    Stretch = Stretch.UniformToFill
                };
                storeBtnImages.Add(preview);
                //Product name.
                TextBlock nameTB = new TextBlock {
                    FontWeight = FontWeights.Bold,
                    FontSize = 36,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = storePages[i].Name,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                //Product price.
                TextBlock priceTB = new TextBlock {
                    FontSize = 24,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = storePages[i].Price
                };
                string cost = storePages[i].Price.ToString().ToLower();
                if (cost == "tbd")
                    priceTB.Text = l.dictionary["mw.s.tbd"];
                else if (cost == "free") priceTB.Text = l.dictionary["mw.s.free"];
                else priceTB.Text = storePages[i].Price.ToString();

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
                    Name = sWhitespace.Replace((storePages[i].Name + storePages[i].ID).Replace("'", 
                    string.Empty), string.Empty)
                };
                main.Click += SB_Click;
                //Add to column.
                ShopBelt.Children.Add(main);
                storeBtns.Add(main);
            }
        }

        private void SB_Click(object sender, RoutedEventArgs e) {
            LocalizationManager l = LocalizationManager.I;
            int ind = storeBtns.IndexOf(sender as Button);
            currentIndex = storePages[ind].ID;
            GameName.Text = storePages[ind].Name;
            DescriptionT.Text = storePages[ind].Description;
            CostT.Text = storePages[ind].Price.ToString();
            if (CostT.Text.ToLower() != "free") {
                BuyBtn.Content = l.dictionary["mw.s.buy"];
                BuyBtn.IsEnabled = true;
            }
            else if (CostT.Text.ToLower() == "tbd") {
                CostT.Text = l.dictionary["mw.s.tbd"];
                BuyBtn.IsEnabled = false;
            }
            else {
                CostT.Text = l.dictionary["mw.s.free"];
                BuyBtn.Content = l.dictionary["mw.s.add"];
                BuyBtn.IsEnabled = true;
            }
            if (storePages[ind].Avaibility != 1) BuyBtn.IsEnabled = false;
            LoadImage(GamePreview, storePages[ind].StoreMedia);
            currentApp = storePages[ind];
            PublisherT.Text = storePages[ind].Publisher;
            if (Database.AppsOnAccount.Contains(storePages[ind].ID.ToString())) BuyBtn.Content =
                l.dictionary["mw.s.open"];
            else if (Database.AppsOnAccount[0] == "*") BuyBtn.Content = l.dictionary["mw.s.open"];
            StorePagePresenter.Visibility = Visibility.Visible;
        }

        void SearchButton_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(searchBar.Text)) {
                for (int i = 0; i < storeBtns.Count; i++) {
                    if (!storeBtns[i].Name.ToLower().Contains(searchBar.Text.ToLower()))
                        storeBtns[i].Visibility = Visibility.Collapsed;
                    else storeBtns[i].Visibility = Visibility.Visible;
                }
            } else {
                for (int i = 0; i < storeBtns.Count; i++) storeBtns[i].Visibility = Visibility.Visible;
            }
        }

        void SearchBar_TextChanged(object sender, TextChangedEventArgs e) {
            if(string.IsNullOrEmpty(searchBar.Text)) searchBarPlaceholder.Visibility = Visibility.Visible;
            else searchBarPlaceholder.Visibility = Visibility.Collapsed;
            if (RegistrySLS.LoadBool("AutoStoreSearch", false)) { searchBtn.Visibility = Visibility.Collapsed; 
                searchBar.Margin = new Thickness(0, 0, 0, 0); SearchButton_Click(sender, e);
                searchBarPlaceholder.Margin = new Thickness(10, 0, 0, 0); }
            else { searchBtn.Visibility = Visibility.Visible; searchBar.Margin = new Thickness(40, 0, 0, 0);
                searchBarPlaceholder.Margin = new Thickness(50, 0, 0, 0); }
        }

        void Scroll_Changed(object sender, ScrollChangedEventArgs e) {
            for (int i = 0; i < storeBtns.Count; i++)
                if (Utils.I.IsControlVisible(storeBtns[i], ShopBelt))
                    LoadImage(storeBtnImages[i], storePages[i].StoreBanner);
        }

        async void LoadImage(Image img, string URL) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
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

        public void BackToMainPage(object sender, RoutedEventArgs e) {
            StorePagePresenter.Visibility = Visibility.Collapsed;
            GamePreview.Source = null;
        }

        void Buy(object sender, RoutedEventArgs e) {
            LocalizationManager l = LocalizationManager.I;
            if (BuyBtn.Content.ToString() == l.dictionary["mw.s.open"]) {
                MainWindow.Instance.gamesControl.Clear();
                MainWindow.Instance.gamesControl.Reload();
                MainWindow.Instance.gamesControl.OpenPageOf(new XGLApp(currentApp.Name, currentApp.Avaibility, 
                    currentApp.Link));
                BackToMainPage(sender, e);
                return;
            }
            string prds = Database.GetValue(App.CurrentAccount, "productsSaved").ToString();
            if (prds == "*") return;
            else if (prds == "-") prds = string.Empty;
            Database.SetValue(Database.DBDataType.DT_PRODUCTSSAVED, prds + currentIndex + ";");
            BuyBtn.Content = l.dictionary["mw.s.open"];
        }

        public void OpenPageOf(long ID) {

            try {
                foreach (StoreBase page in storePages)
                    if (page.ID == ID) currentApp = page;
                BackToMainPage(null, null);
                SB_Click(storeBtns[storePages.IndexOf(currentApp)], null);
                MainWindow.Instance.CollapseAllPages(MainWindow.Instance.StorePg);
                MainWindow.Instance.publisherControl.Clear();
            }
            catch {
                MessageBox.Show("Издатель не обнаружен", "XGLauncher");
            }

        }

        void PublisherT_MouseDown(object sender, MouseButtonEventArgs e) {

            MainWindow.Instance.publisherControl.Clear();
            MainWindow.Instance.publisherControl.PublisherName = storePages[int.Parse(currentIndex.ToString()) - 1].Publisher;
            MainWindow.Instance.publisherControl.Reload();
            MainWindow.Instance.CollapseAllPages(MainWindow.Instance.PublisherPg);
            MainWindow.Instance.curP = 6;
        }

    }

}
