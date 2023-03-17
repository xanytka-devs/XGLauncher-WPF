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
using XGL.Networking.Database;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using static XGL.Networking.Database.Database;

namespace XGL.Pages.LW {

    public enum NewsType {
        Announcement,
        Update,
        MajorUpdate,
        Build,
        Alpha,
        Beta,
        Release,
    }

    public class NewsBase {

        public long ID { get; private set; }
        public long ProductID { get; private set; }
        public string Name { get; set; }
        public string Body { get; private set; }
        public string Icon { get; private set; }
        public string MainIcon { get; private set; }
        public DateTime CreationDate { get; private set; }
        public int Likes { get; private set; }

        public NewsBase(long iD, long productID, string name, string body, DateTime creationDate, int likes, string icon, string mainIcon) {
            ID = iD;
            ProductID = productID;
            Name = name;
            Body = body;
            CreationDate = creationDate;
            Likes = likes;
            Icon = icon;
            MainIcon = mainIcon;
        }
    }

    /// <summary>
    /// Логика взаимодействия для News.xaml
    /// </summary>

    public partial class News : UserControl {

        readonly List<StackPanel> panels = new List<StackPanel>();
        readonly List<Button> newsBtns = new List<Button>();
        readonly List<NewsBase> news = new List<NewsBase>();
        readonly List<Image> newsBtnImages = new List<Image>();

        public News() {
            InitializeComponent();
            panels.Add(pan0);
            panels.Add(pan1);
            panels.Add(pan2);
            panels.Add(pan3);
            panels.Add(pan4);
        }

        public void Reload() {
            //Get data from database.
            LoadDataFromDB();
            //Generate news buttons.
            GenerateNewsButtons();
        }

        public void Clear() {
            for (int i = 0; i < panels.Count; i++)
                panels[i].Children.Clear();
            newsBtns.Clear();
            newsBtnImages.Clear();
            news.Clear();
        }

        void LoadDataFromDB() {
            string ids = GetAppIDs();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `news` WHERE `productID` IN ({ids})", Connection);
            if(ids == "*") command = new MySqlCommand($"SELECT * FROM `news`", Connection);
            else if (ids != "*" && ids != "-" && ids.Length > 0) command = new MySqlCommand($"SELECT * FROM `news` WHERE `productID` IN (0,{ids})", Connection);
            else if(ids == "-") command = new MySqlCommand($"SELECT * FROM `news` WHERE `productID` = 0", Connection);
            try  {
                //Open connection and read everything, what needed.
                OpenConnection();
                MySqlDataReader dr = command.ExecuteReader();
                while (dr.Read()) {
                    news.Add(new NewsBase(dr.GetInt64("id"),
                                          dr.GetInt64("productID"),
                                          dr.GetString("name"),
                                          dr.GetString("body"),
                                          dr.GetDateTime("createdOn"),
                                          dr.GetInt32("likes"),
                                          dr.GetString("icon"),
                                          dr.GetString("mainIcon")));
                }
                //Close reader and connection.
                dr.Close();
                CloseConnection();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
                return;
            }
        }

        void GenerateNewsButtons() {
            int curNP = 0;

            for (int i = 0; i < news.Count; i++) {
                //INNER//
                //Border for text visibility.
                Border visArea = new Border() {
                    Background = new LinearGradientBrush(Color.FromRgb(0, 0, 0), Color.FromArgb(0, 0, 0, 0),
                        new Point(0.5, 0.9), new Point(0.5, 0))
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
                    Width = 180,
                    Height = 180,
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
                if (curNP == 5) curNP = 0;
            }
            Scroll_Changed(ScrollBar, null);
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

        void ExitNewsPost(object sender, RoutedEventArgs e) { 
            newsPost.Visibility = Visibility.Collapsed;
            BGImage.Source = null;
        }

        private void Scroll_Changed(object sender, ScrollChangedEventArgs e) {
            for (int i = 0; i < newsBtns.Count; i++) {
                if (Utils.IsControlVisible(newsBtns[i])) {
                    LoadImageBtn(i, newsBtnImages[i]);
                }
            }
        }

        async void LoadImage(int i) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", news[i].Icon.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
                await Utils.DownloadFileAsync(news[i].Icon.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            BGImage.Source = logo;
        }

        async void LoadImageBtn(int i, Image img) {
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", news[i].MainIcon.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg))
                await Utils.DownloadFileAsync(news[i].MainIcon.Split('{')[1], nameOfImg);
            BitmapImage logo = new BitmapImage();
            logo.CacheOption = BitmapCacheOption.OnLoad;
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            img.Source = logo;
        }

    }

}
