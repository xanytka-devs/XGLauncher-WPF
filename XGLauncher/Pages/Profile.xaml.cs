using ColorPicker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XGL.Networking.Database;
using XGL.SLS;
using XGLauncher.Dialogs;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>

    public partial class Profile : UserControl {

        public class ColorSelectorAndText {
            public SquarePicker pickerSqr;
            public HexColorTextBox pickerTB;
            public TextBlock text;
            public ColorSelectorAndText(SquarePicker _pickerSqr, HexColorTextBox _pickerTB, TextBlock _text) {
                pickerSqr = _pickerSqr;
                pickerTB = _pickerTB;
                text = _text;
            }
        }

        public Profile() {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            InitializeComponent();
            Loaded += Profile_Loaded;
        }

        bool isEditingProfile = false;
        bool isEditingColors = false;
        bool canMoveBadges = false;
        string[] personalData;
        readonly byte[] outRGB = { 0, 0, 0 };
        string vAlg = "top";
        string hAlg = "right";
        int cpi = 0;
        //List<ColorSelectorAndText> pages = new List<ColorSelectorAndText>();

        private void Profile_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= Profile_Loaded;
            //Load personalization data.
            if (RegistrySLS.LoadString("Personalization") != "False")
                //Load from save file (if exists).
                personalData = RegistrySLS.LoadString("Personalization").Replace(',', '.').Split('-');
            else if (!App.OnlineMode)
                //Load from database (if not in offline or null modes).
                personalData = Database.GetValue(App.CurrentAccount, "personalizationChoices")
                        .ToString().Replace(',', '.').Split('-');
            //Else just create sample values string.
            else personalData = new string[] { "0", "0", "0", "top", "right" };
            //Define texts of UI elements.
            if (string.IsNullOrEmpty(RegistrySLS.LoadString("Description")) ||
                    RegistrySLS.LoadString("Description") == "INS") {
                RegistrySLS.Save("Description", Database.GetValue(App.CurrentAccount, "description").ToString());
            }
            profileT.Text = RegistrySLS.LoadString("Username");
            profileTB.Text = RegistrySLS.LoadString("Username");
            profileIDT.Text = RegistrySLS.LoadString("LastID").Split(';')[0];
            profileDescT.Text = RegistrySLS.LoadString("Description");
            profileDescTB.Text = RegistrySLS.LoadString("Description");
            //Define description.
            if (string.IsNullOrEmpty(RegistrySLS.LoadString("Description")) ||
                RegistrySLS.LoadString("Description") == "INS") {
                //If there isn't any description saved - 
                // hide description text box.
                profileAddDesc.Visibility = Visibility.Collapsed;
                profileDescT.Visibility = Visibility.Collapsed;
                profileDescT.Text = string.Empty;
                profileDescTB.Text = string.Empty;
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
            bGColorPickerBody.SelectedColor = Color.FromRgb(outRGB[0], outRGB[1], outRGB[2]);
            bGColorPickerHEX.SelectedColor = Color.FromRgb(outRGB[0], outRGB[1], outRGB[2]);
            //Define badge location.
            //Vertical
            if (personalData[3] == "bottom")
                badgePanel.VerticalAlignment = VerticalAlignment.Bottom;
            else {
                badgePanel.VerticalAlignment = VerticalAlignment.Top;
                vAlg = "top";
            }
            //Horizontal
            if (personalData[4] == "left")
                badgePanel.HorizontalAlignment = HorizontalAlignment.Left;
            else {
                badgePanel.HorizontalAlignment = HorizontalAlignment.Right;
                hAlg = "right";
            }
            LoadImage();
        }

        async void LoadImage() {
            string URL = Database.GetValue(App.CurrentAccount, "icon").ToString();
            string nameOfImg = Path.Combine(App.CurrentFolder, "cache", URL.Split('{')[0] + ".jpg");
            if (!File.Exists(nameOfImg)) {
                Thread.Sleep(1000);
                await Utils.DownloadFileAsync(URL.Split('{')[1], nameOfImg);
            } else Thread.Sleep(1000);
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(nameOfImg);
            logo.EndInit();
            logo.Freeze();
            profilePgIcon.Source = logo;
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e) {
            if (!isEditingProfile) {
                //Change visibility.
                changeBGColorBtn.Visibility = Visibility.Visible;
                profileT.Visibility = Visibility.Collapsed;
                profileTB.Visibility = Visibility.Visible;
                profileAddDesc.Visibility = Visibility.Visible;
                profileDescT.Visibility = Visibility.Collapsed;
                profileDescTB.Visibility = Visibility.Visible;
                cUB_EI.Visibility = Visibility.Collapsed;
                cUB_SI.Visibility = Visibility.Visible;
                photoChangeBtnVis.Visibility = Visibility.Visible;
                if(RegistrySLS.LoadBool("CustomProfileImage", false)) photoChangeBtn.Visibility = Visibility.Visible;
                if (profileDescTB.Text == "INS") profileDescTB.Text = string.Empty;
                //If can edit badges -
                if (canMoveBadges) { 
                    // show edit icons.
                    hdirBP.Visibility = Visibility.Visible;
                    vdirBP.Visibility = Visibility.Visible;
                }
                //Set editing to true.
                isEditingProfile = true;
            } else {
                if (!string.IsNullOrEmpty(profileTB.Text)) {
                    if(profileTB.Text.Length > 25) { 
                        MessageBox.Show("Username is too big. Maximum size must be 25 or less characters.", "XGLauncher", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(profileDescTB.Text)) profileDescTB.Text = "INS";
                    else if (profileDescTB.Text.Length > 255) {
                        MessageBox.Show("Description is too big. Maximum size must be 255 or less characters.", "XGLauncher",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    //Save data.
                    SaveInfo();
                    //Change texts.
                    profileT.Text = profileTB.Text;
                    profileDescT.Text = profileDescTB.Text;
                    MainWindow.Instance.profile.Text = profileTB.Text;
                    //Change visibility.
                    profileT.Visibility = Visibility.Visible;
                    profileTB.Visibility = Visibility.Collapsed;
                    profileDescT.Visibility = Visibility.Visible;
                    profileDescTB.Visibility = Visibility.Collapsed;
                    cUB_EI.Visibility = Visibility.Visible;
                    cUB_SI.Visibility = Visibility.Collapsed;
                    bGColorPicker.Visibility = Visibility.Collapsed;
                    changeBGColorBtn.Visibility = Visibility.Collapsed;
                    photoChangeBtnVis.Visibility = Visibility.Collapsed;
                    photoChangeBtn.Visibility = Visibility.Collapsed;
                    //If description is empty - hide it.
                    if (string.IsNullOrEmpty(profileDescTB.Text) || profileDescTB.Text == "INS") {
                        profileDescT.Visibility = Visibility.Collapsed;
                        profileAddDesc.Visibility = Visibility.Collapsed;
                    } else profileAddDesc.Visibility = Visibility.Visible;
                    //If can edit badges -
                    if (canMoveBadges) {
                        // hide edit icons.
                        hdirBP.Visibility = Visibility.Collapsed;
                        vdirBP.Visibility = Visibility.Collapsed;
                    }
                    //Set editing to false.
                    isEditingColors = false;
                    isEditingProfile = false;
                }
            }
        }

        void SetVisibility(Visibility vis) {


        }

        private void ChangeBGColorBtn_Click(object sender, RoutedEventArgs e) {

            if (!isEditingColors) {
                bGColorPicker.Visibility = Visibility.Visible;
                isEditingColors = true;
                return;
            }
            bGColorPicker.Visibility = Visibility.Collapsed;
            isEditingColors = false;

        }

        private void BGColorPickerBody_ColorChanged(object sender, RoutedEventArgs e) {
            if (sender != null) {
                if (sender.ToString() == bGColorPickerBody.ToString())
                    bGColorPickerHEX.SelectedColor = bGColorPickerBody.SelectedColor;
                else
                    bGColorPickerBody.SelectedColor = bGColorPickerHEX.SelectedColor;
            }

            bgColor.Color = Color.FromRgb((byte)bGColorPickerBody.Color.RGB_R,
                (byte)bGColorPickerBody.Color.RGB_G, (byte)bGColorPickerBody.Color.RGB_B);
            bgFadeColor.Color = Color.FromArgb(0,
                (byte)bGColorPickerBody.Color.RGB_R, (byte)bGColorPickerBody.Color.RGB_G, (byte)bGColorPickerBody.Color.RGB_B);
        }

        private void BGPageColorPickerBody_ColorChanged(object sender, RoutedEventArgs e) {
            if (sender != null) {
                if (sender.ToString() == bGPageColorPickerBody.ToString())
                    bGPageColorPickerHEX.SelectedColor = bGColorPickerBody.SelectedColor;
                else
                    bGPageColorPickerBody.SelectedColor = bGColorPickerHEX.SelectedColor;
            }

            pageBg.Background = new SolidColorBrush(Color.FromRgb((byte)bGPageColorPickerBody.Color.RGB_R,
                (byte)bGPageColorPickerBody.Color.RGB_G, (byte)bGPageColorPickerBody.Color.RGB_B));
        }

        public void Reload() {
            //Define current and empty badge variables.
            string emptyBadgeCollection = "0000000000000000000000000";
            char[] badges = emptyBadgeCollection.ToCharArray();
            if (!App.OnlineMode && RegistrySLS.LoadString("Description", "INS") != "INS")
                badges = Database.GetValue(App.CurrentAccount, "badges").ToString().ToCharArray();
            //Check for badges.
            //NOTE #1: 0 and 1-s used like bool variable.
            //NOTE #2: If the user has at least one badge,
            // then the first value will always be one.
            if (badges[0] == '1') canMoveBadges = true;
            if (badges[1] == '0') modBadge.Visibility = Visibility.Collapsed;
            if (badges[2] == '0') premiumBadge.Visibility = Visibility.Collapsed;
            if (badges[3] == '0') lightningBadge.Visibility = Visibility.Collapsed;
        }

        private void VdirBP_Click(object sender, RoutedEventArgs e) {
            //If align was assigned to top - change it to bottom.
            //Else change it to top
            if (badgePanel.VerticalAlignment == VerticalAlignment.Top) {
                badgePanel.VerticalAlignment = VerticalAlignment.Bottom;
                vAlg = "bottom";
            } else {
                badgePanel.VerticalAlignment = VerticalAlignment.Top;
                vAlg = "top";
            }
        }

        private void HdirBP_Click(object sender, RoutedEventArgs e) {
            //If align was assigned to left - change it to right.
            //Else change it to left
            if (badgePanel.HorizontalAlignment == HorizontalAlignment.Left) {
                badgePanel.HorizontalAlignment = HorizontalAlignment.Right;
                hAlg = "right";
            } else {
                badgePanel.HorizontalAlignment = HorizontalAlignment.Left;
                hAlg = "left";
            }
        }

        void SaveInfo() {
            //Define colors
            outRGB[0] = byte.Parse(Math.Ceiling(bGColorPickerBody.Color.RGB_R).ToString());
            outRGB[1] = byte.Parse(Math.Ceiling(bGColorPickerBody.Color.RGB_G).ToString());
            outRGB[2] = byte.Parse(Math.Ceiling(bGColorPickerBody.Color.RGB_B).ToString());
            //Define output.
            string outputed = outRGB[0].ToString() + '-' + outRGB[1].ToString() + '-' + outRGB[2].ToString() + '-' + 
                /*outRGB[3].ToString() + '-' + outRGB[4].ToString() + '-' + outRGB[5].ToString() + '-' +*/ vAlg + '-' + hAlg;
            //Save to database.
            Database.SetValue(Database.DBDataType.DT_DESCRIPTION, profileDescTB.Text);
            Database.SetValue(Database.DBDataType.DT_PERSONALIZATIONVALUES, outputed);
            Database.SetValue(Database.DBDataType.DT_USERNAME, profileTB.Text);
            //Save to app settings.
            RegistrySLS.Save("Username", profileTB.Text);
            RegistrySLS.Save("Personalization", outputed);
            RegistrySLS.Save("Description", profileDescTB.Text);
        }

        public void ApplyLocalization(string localization) {

            switch (localization) {
                case "ru-RU":
                    profileAddDesc.Text = "Описание";
                    bGColorPickerText.Text = "Цвет Фона профиля";
                    break;
                case "en-US":
                    profileAddDesc.Text = "Description";
                    bGColorPickerText.Text = "Profile Color";
                    break;
                case "es":
                    profileAddDesc.Text = "Descripción";
                    bGColorPickerText.Text = "Color del Perfil";
                    break;
                case "ru-IM":
                    profileAddDesc.Text = "Описаніе";
                    bGColorPickerText.Text = "Цвѣтъ Фона профиля";
                    break;
            }

        }

        void NextPicker(object sender, RoutedEventArgs e) {

            switch (cpi) {
                case 0:
                    CP_Next.IsEnabled = false;
                    CP_Last.IsEnabled = true;
                    CP_Next.Opacity = 0.5f;
                    CP_Last.Opacity = 1;
                    cpi = 1;
                    break;
            }
            SetPage();

        }

        void LastPicker(object sender, RoutedEventArgs e) {
            switch (cpi) {
                case 1:
                    CP_Next.IsEnabled = true;
                    CP_Last.IsEnabled = false;
                    CP_Next.Opacity = 1;
                    CP_Last.Opacity = 0.5f;
                    cpi = 0;
                    break;
            }
            //SetPage();
        }

        void SetPage() {
            /*for (int i = 0; i < pages; i++) { }
            switch (cpi) {
                case 1:         
                    break;
            }*/
        }

        void OpenImageDialog(object sender, RoutedEventArgs e) { TempURLDialog td = new TempURLDialog(); td.Show(); }

    }

}
