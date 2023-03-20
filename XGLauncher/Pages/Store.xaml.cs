using MySql.Data.MySqlClient;
using static XGL.Networking.Database.Database;
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
using XGL.Networking.Database;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Store.xaml
    /// </summary>
    
    public class StoreBase {

        public long ID { get; private set; }
        public string Name { get; set; }
        public long PublisherID { get; private set; }
        public string Publisher { get; set; }
        public int Avaibility { get; private set; }
        public string Genres { get; private set; }
        public string Specification { get; private set; }
        public string StoreBanner { get; private set; }
        public string StoreMedia { get; private set; }
        public string XanPage { get; private set; }
        public string Price { get; private set; }
        public float Rating { get; private set; }
        public string Description { get; private set; }
        public string Link { get; private set; }

        public StoreBase(long iD, string name, long publisherID,
            int avaibility, string genres, string specification,
            string icon, string previewIcon, string xanPage, float rating, string price,
            string description, string link) {
            ID = iD;
            Name = name;
            PublisherID = publisherID;
            Avaibility = avaibility;
            Genres = genres;
            Specification = specification;
            StoreBanner = icon;
            StoreMedia = previewIcon;
            XanPage = xanPage;
            Rating = rating;
            Price = price;
            Description = description;
            Link = link;
        }
    }


    public partial class Store : UserControl {

        readonly List<Button> storeBtns = new List<Button>();
        readonly List<Image> storeBtnImages = new List<Image>();
        readonly List<StoreBase> storePages = new List<StoreBase>();
        StoreBase currentApp;
        long currentIndex;

        public class ShopButton {
            public Button Preview;
            public Page BodyPage;
        }

        public Store() {
            InitializeComponent();
        }

        public void Reload() {
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
        }

        public void Clear() {
            BackToMainPage(null, null);
            storePages.Clear();
            storeBtns.Clear();
            storeBtnImages.Clear();
            ShopBelt.Children.Clear();
        }

        public void ApplyLocalization(string localization) {
            switch (localization) {
                case "ru-RU":
                    searchBarPlaceholder.Text = "Поиск";
                    break;
                case "en-US":
                    searchBarPlaceholder.Text = "Search";
                    break;
                case "es":
                    searchBarPlaceholder.Text = "Suche";
                    break;
            }
        }
        void LoadContent() {
            //string ids = GetAppIDs();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `xgl_products`", Connection);
            try {
                //Open connection and read everything, what needed.
                OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while (dr.Read()) {
                    storePages.Add(new StoreBase(dr.GetInt64("id"),
                                                 dr.GetString("name"),
                                                 dr.GetInt64("publisherID"),
                                                 dr.GetInt16("availability"),
                                                 dr.GetString("genres"),
                                                 dr.GetString("specification"),
                                                 dr.GetString("storeBanner"),
                                                 dr.GetString("storeMedia"),
                                                 dr.GetString("xanPage"),
                                                 dr.GetFloat("rating"),
                                                 dr.GetString("price"),
                                                 dr.GetString("description"),
                                                 dr.GetString("latestDownloadLinks")));
                }
                //Close reader and connection.
                dr.Close();
                CloseConnection();
                CheckItems();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
                return;
            }
        }

        int cii = 0;
        void CheckItems() {
            if (cii == storePages.Count) return;
            if (storePages[cii].Avaibility == 2) storePages.Remove(storePages[cii]);
            else cii++;
            CheckItems();
        }

        void GenerateButtons() {
            for (int i = 0; i < storePages.Count; i++) {
                //INNER//
                //Preview image.
                Image preview = new Image {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 500,
                    Stretch = Stretch.UniformToFill
                };
                storeBtnImages.Add(preview);
                //Product name.
                TextBlock nameTB = new TextBlock {
                    FontWeight = FontWeights.Bold,
                    FontSize = 36,
                    Margin = new Thickness(505, 5, 5, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = storePages[i].Name
                };
                //Product price.
                TextBlock priceTB = new TextBlock {
                    FontSize = 24,
                    Margin= new Thickness(5, 5, 15, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = storePages[i].Price
                };
                //Holder.
                Grid inner = new Grid();
                inner.Children.Add(preview);
                inner.Children.Add(nameTB);
                inner.Children.Add(priceTB);
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
                    Name = sWhitespace.Replace((storePages[i].Name + storePages[i].ID).Replace("'", string.Empty), string.Empty)
                };
                main.Click += (s, e) => {
                    int ind = storeBtns.IndexOf(s as Button);
                    try {
                        //Open connection and read everything, what needed.
                        OpenConnection();
                        MySqlCommand command = new MySqlCommand($"SELECT `name` FROM `xgl_publishers` WHERE `id` = {storePages[ind].PublisherID}", Connection);
                        MySqlDataReader dr = command.ExecuteReader();
                        while (dr.Read()) {
                            storePages[ind].Publisher = dr.GetString("name");
                        }
                        //Close reader and connection.
                        dr.Close();
                        CloseConnection();
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message);
                    }
                    currentIndex = storePages[ind].ID;
                    GameName.Text = storePages[ind].Name;
                    DescriptionT.Text = storePages[ind].Description;
                    CostT.Text = storePages[ind].Price.ToString();
                    if (CostT.Text.ToLower() != "free") BuyBtn.Content = "Buy";
                    else if (CostT.Text.ToLower() == "tbd") BuyBtn.IsEnabled = false;
                    else { BuyBtn.Content = "Add to library"; BuyBtn.IsEnabled = true; }
                    if (storePages[ind].Avaibility != 1) BuyBtn.IsEnabled = false;
                    LoadImage(GamePreview, storePages[ind].StoreMedia);
                    currentApp = storePages[ind];
                    PublisherT.Text = storePages[ind].Publisher;
                    if(AppsOnAccount.Contains(storePages[ind].ID.ToString())) BuyBtn.Content = "Open in library";
                    else if (AppsOnAccount[0] == "*") BuyBtn.Content = "Open in library";
                    StorePagePresenter.Visibility = Visibility.Visible;
                };
                //Add to column.
                ShopBelt.Children.Add(main);
                storeBtns.Add(main);
            }
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
            for (int i = 0; i < storeBtns.Count; i++) {
                if (Utils.IsControlVisible(storeBtns[i])) {
                    LoadImage(storeBtnImages[i], storePages[i].StoreBanner);
                }
            }
        }

        async void LoadImage(Image img, string URL) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
                await Utils.DownloadFileAsync(URL.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.CacheOption = BitmapCacheOption.OnLoad;
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
            if(BuyBtn.Content.ToString() == "Open in library") {
                MainWindow.Instance.gamesControl.OpenPageOf(new XGLApp(currentApp.Name, currentApp.Avaibility, currentApp.Link));
                BackToMainPage(sender, e);
                return;
            }
            string prds = Database.GetValue(App.CurrentAccount, "productsSaved").ToString();
            if (prds == "*") return;
            else if (prds == "-") prds = string.Empty;
            Database.SetValue(DBDataType.DT_PRODUCTSSAVED, prds + currentIndex + ";");
            BuyBtn.Content = "Open in library";
        }

    }
}
