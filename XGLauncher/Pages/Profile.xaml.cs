using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.IO;
using XGL.Networking;
using XGL.SLS;
using Org.BouncyCastle.Crypto;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    
    public partial class Profile : UserControl {

        public Profile() {

            if(DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            InitializeComponent();

        }

        public long ID = 1;
        string[] personalData;
        readonly byte[] outRGB = { 255, 128, 44 };
        Account CurrentAccount = new Account("", "");

        public void Profile_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= Profile_Loaded;
            CurrentAccount = new Account(Database.GetValue("id", ID, "login").ToString(), Database.GetValue("id", ID, "password").ToString());
            //Load from database (if not in offline or null modes).
            personalData = Database.GetValue(CurrentAccount, "personalizationChoices")
                .ToString().Replace(',', '.').Split('-');
            //Define texts of UI elements.
            profileT.Text = Database.GetValue(CurrentAccount, "username").ToString();
            profileIDT.Text = ID.ToString();
            profileDescT.Text = Database.GetValue(CurrentAccount, "description").ToString();
            //Define description.
            if(profileDescT.Text == "INS") {
                //If there isn't any description saved - 
                // hide description text box.
                profileAddDesc.Visibility = Visibility.Collapsed;
                profileDescT.Visibility = Visibility.Collapsed;
                profileDescT.Text = string.Empty;
            } else {
                //Else show description text box.
                profileAddDesc.Visibility = Visibility.Visible;
                profileDescT.Visibility = Visibility.Visible;
            }
            //Define colors.
            for (int i = 0; i < 3; i++) outRGB[i] = byte.Parse(personalData[i]);
            //Apply colors.
            bgColor.Color = Color.FromRgb(outRGB[0], outRGB[1], outRGB[2]);
            bgFadeColor.Color = Color.FromArgb(0, outRGB[0], outRGB[1], outRGB[2]);
            //Define badge location.
            //Vertical
            if(personalData[3] == "bottom")
                badgePanel.VerticalAlignment = VerticalAlignment.Bottom;
            else badgePanel.VerticalAlignment = VerticalAlignment.Top;
            //Horizontal
            if(personalData[4] == "left")
                badgePanel.HorizontalAlignment = HorizontalAlignment.Left;
            else badgePanel.HorizontalAlignment = HorizontalAlignment.Right;
            LoadImage();
        }

        void LoadImage() {
            try {
                string URL = string.Empty;
                if(App.IsFirstRun) URL = "default{https://drive.google.com/uc?export=download&id=1hKSUYQgTaJIp8V-coY8Y8Bmod0eIupzy";
                else URL = Database.GetValue(CurrentAccount, "icon").ToString();
                string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(nameOfImg);
                logo.EndInit();
                logo.Freeze();
                profilePgIcon.Source = logo;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                LoadImage();
            }
        }

        public void Reload() {
            Profile_Loaded(null, null);
            //Define current and empty badge variables.
            string emptyBadgeCollection = "0000000000000000000000000";
            char[] badges = emptyBadgeCollection.ToCharArray();
            if(RegistrySLS.LoadString("Description", "INS") != "INS")
                badges = Database.GetValue(CurrentAccount, "badges").ToString().ToCharArray();
            //Check for badges.
            //NOTE #1: 0 and 1-s used like bool variable.
            //NOTE #2: If the user has at least one badge,
            // then the first value will always be one.
            if(badges[1] == '0') verified.Visibility = Visibility.Collapsed;
            if(badges[2] == '0') modBadge.Visibility = Visibility.Collapsed;
            if(badges[3] == '0') premiumBadge.Visibility = Visibility.Collapsed;
            if(badges[4] == '0') betaBadge.Visibility = Visibility.Collapsed;
        }

        public void ApplyLocalization() {
            LocalizationManager l = LocalizationManager.I;
            profileAddDesc.Text = l.dictionary["gn.desc"];
            verified.ToolTip = l.dictionary["mw.p.ver"];
            modBadge.ToolTip = l.dictionary["mw.p.mod"];
            premiumBadge.ToolTip = l.dictionary["mw.p.prem"];
            betaBadge.ToolTip = l.dictionary["mw.p.beta"];
        }

    }

}
