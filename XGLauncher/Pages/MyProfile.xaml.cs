using ColorPicker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XGL.Networking;
using XGL.SLS;
using XGLauncher.Dialogs;

namespace XGL.Pages.LW {

    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>

    public partial class MyProfile : UserControl {

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

        public MyProfile() {
            if(DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            InitializeComponent();
            Loaded += Profile_Loaded;
        }

        bool isEditingProfile = false;
        bool isEditingColors = false;
        bool canMoveBadges = false;
        bool isVerified = true;
        string[] personalData;
        readonly byte[] outRGB = { 255, 128, 44 };
        string vAlg = "top";
        string hAlg = "right";

        private void Profile_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= Profile_Loaded;
            //Load personalization data.
            if(RegistrySLS.LoadString("Personalization") != "False")
                //Load from save file (if exists).
                personalData = RegistrySLS.LoadString("Personalization").Replace(',', '.').Split('-');
            else if(!App.IsFirstRun)
                //Load from database (if not in offline or null modes).
                personalData = Database.GetValue(App.CurrentAccount, "personalizationChoices")
                        .ToString().Replace(',', '.').Split('-');
            //Else just create sample values string.
            else personalData = new string[] { "255", "128", "44", "top", "right" };
            //Define texts of UI elements.
            if(string.IsNullOrEmpty(RegistrySLS.LoadString("Description")) ||
                    RegistrySLS.LoadString("Description") == "INS") {
                if(!App.IsFirstRun)
                    RegistrySLS.Save("Description", Database.GetValue(App.CurrentAccount, "description").ToString());
                else RegistrySLS.Save("Description", "INS");
            }
            profileT.Text = RegistrySLS.LoadString("Username");
            profileTB.Text = RegistrySLS.LoadString("Username");
            profileIDT.Text = RegistrySLS.LoadString("LastID").Split(';')[0];
            profileDescT.Text = RegistrySLS.LoadString("Description");
            profileDescTB.Text = RegistrySLS.LoadString("Description");
            //Define description.
            if(string.IsNullOrEmpty(RegistrySLS.LoadString("Description")) ||
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
            if(personalData[3] == "bottom")
                badgePanel.VerticalAlignment = VerticalAlignment.Bottom;
            else {
                badgePanel.VerticalAlignment = VerticalAlignment.Top;
                vAlg = "top";
            }
            //Horizontal
            if(personalData[4] == "left")
                badgePanel.HorizontalAlignment = HorizontalAlignment.Left;
            else {
                badgePanel.HorizontalAlignment = HorizontalAlignment.Right;
                hAlg = "right";
            }
            LoadImage();
        }

        void LoadImage() {
            try {
                string URL = string.Empty;
                if(App.IsFirstRun) URL = "default{https://drive.google.com/uc?export=download&id=1hKSUYQgTaJIp8V-coY8Y8Bmod0eIupzy";
                else URL = Database.GetValue(App.CurrentAccount, "icon").ToString();
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

        private void EditProfile_Click(object sender, RoutedEventArgs e) {
            if(!isEditingProfile) {
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
                photoChangeBtn.Visibility = Visibility.Visible;
                if(profileDescTB.Text == "INS") profileDescTB.Text = string.Empty;
                //If can edit badges -
                if(canMoveBadges) { 
                    // show edit icons.
                    hdirBP.Visibility = Visibility.Visible;
                    vdirBP.Visibility = Visibility.Visible;
                }
                verified.Visibility = Visibility.Collapsed;
                //Set editing to true.
                isEditingProfile = true;
            } else {
                LocalizationManager l = LocalizationManager.I;
                if(!string.IsNullOrEmpty(profileTB.Text)) {
                    if(profileTB.Text.Length > 25) {
                        MessageBox.Show(l.dictionary["mw.p.tlu"], "XGLauncher", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if(profileTB.Text.Length > 10) {
                        MessageBox.Show(l.dictionary["mw.p.plu"], "XGLauncher",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    if(string.IsNullOrEmpty(profileDescTB.Text)) profileDescTB.Text = "INS";
                    else if(profileDescTB.Text.Length > 255) {
                        MessageBox.Show(l.dictionary["mw.p.tld"], "XGLauncher",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    //Save data.
                    SaveInfo();
                    //Change texts.
                    profileT.Text = profileTB.Text;
                    profileDescT.Text = profileDescTB.Text;
                    MainWindow.Instance.myProfile.Text = profileTB.Text;
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
                    if(string.IsNullOrEmpty(profileDescTB.Text) || profileDescTB.Text == "INS") {
                        profileDescT.Visibility = Visibility.Collapsed;
                        profileAddDesc.Visibility = Visibility.Collapsed;
                    } else profileAddDesc.Visibility = Visibility.Visible;
                    //If can edit badges -
                    if(canMoveBadges) {
                        // hide edit icons.
                        hdirBP.Visibility = Visibility.Collapsed;
                        vdirBP.Visibility = Visibility.Collapsed;
                    }
                    if(isVerified) verified.Visibility = Visibility.Visible;
                    //Set editing to false.
                    isEditingColors = false;
                    isEditingProfile = false;
                }
            }
        }

        private void ChangeBGColorBtn_Click(object sender, RoutedEventArgs e) {

            if(!isEditingColors) {
                bGColorPicker.Visibility = Visibility.Visible;
                isEditingColors = true;
                return;
            }
            bGColorPicker.Visibility = Visibility.Collapsed;
            isEditingColors = false;

        }

        private void BGColorPickerBody_ColorChanged(object sender, RoutedEventArgs e) {
            if(sender != null) {
                if(sender.ToString() == bGColorPickerBody.ToString())
                    bGColorPickerHEX.SelectedColor = bGColorPickerBody.SelectedColor;
                else
                    bGColorPickerBody.SelectedColor = bGColorPickerHEX.SelectedColor;
            }

            bgColor.Color = Color.FromRgb((byte)bGColorPickerBody.Color.RGB_R,
                (byte)bGColorPickerBody.Color.RGB_G, (byte)bGColorPickerBody.Color.RGB_B);
            bgFadeColor.Color = Color.FromArgb(0,
                (byte)bGColorPickerBody.Color.RGB_R, (byte)bGColorPickerBody.Color.RGB_G, (byte)bGColorPickerBody.Color.RGB_B);
        }

        public void Reload() {
            //Define current and empty badge variables.
            string emptyBadgeCollection = "0000000000000000000000000";
            char[] badges = emptyBadgeCollection.ToCharArray();
            if(RegistrySLS.LoadString("Description", "INS") != "INS")
                badges = Database.GetValue(App.CurrentAccount, "badges").ToString().ToCharArray();
            //Check for badges.
            //NOTE #1: 0 and 1-s used like bool variable.
            //NOTE #2: If the user has at least one badge,
            // then the first value will always be one.
            if(badges[0] == '1' | App.IsPremium) canMoveBadges = true;
            if(badges[1] == '0') { isVerified = false; verified.Visibility = Visibility.Collapsed; }
            if(badges[2] == '0') modBadge.Visibility = Visibility.Collapsed;
            if(badges[3] == '0') betaBadge.Visibility = Visibility.Collapsed;
            if(!App.IsPremium) premiumBadge.Visibility = Visibility.Collapsed;
        }

        private void VdirBP_Click(object sender, RoutedEventArgs e) {
            //If align was assigned to top - change it to bottom
            //else change it to top
            if(badgePanel.VerticalAlignment == VerticalAlignment.Top) {
                badgePanel.VerticalAlignment = VerticalAlignment.Bottom;
                vAlg = "bottom";
            } else {
                badgePanel.VerticalAlignment = VerticalAlignment.Top;
                vAlg = "top";
            }
        }

        private void HdirBP_Click(object sender, RoutedEventArgs e) {
            //If align was assigned to left - change it to right
            //else change it to left
            if(badgePanel.HorizontalAlignment == HorizontalAlignment.Left) {
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
            string outputed = outRGB[0].ToString() + '-' + outRGB[1].ToString() + '-' + outRGB[2].ToString() + '-' + vAlg + '-' + hAlg;
            //Save to database.
            Database.SetValue(Database.DBDataType.DT_DESCRIPTION, profileDescTB.Text);
            Database.SetValue(Database.DBDataType.DT_PERSONALIZATIONVALUES, outputed);
            Database.SetValue(Database.DBDataType.DT_USERNAME, profileTB.Text);
            //Save to app settings.
            RegistrySLS.Save("Username", profileTB.Text);
            RegistrySLS.Save("Personalization", outputed);
            RegistrySLS.Save("Description", profileDescTB.Text);
        }

        public void ApplyLocalization() {

            LocalizationManager l = LocalizationManager.I;

            profileAddDesc.Text = l.dictionary["gn.desc"];
            bGColorPickerText.Text = l.dictionary["mw.p.pbgc"];
            verified.ToolTip = l.dictionary["mw.p.ver"];
            modBadge.ToolTip = l.dictionary["mw.p.mod"];
            premiumBadge.ToolTip = l.dictionary["mw.p.prem"];
            betaBadge.ToolTip = l.dictionary["mw.p.beta"];

        }

        void OpenImageDialog(object sender, RoutedEventArgs e) { TempURLDialog td = new TempURLDialog(); td.Show(); }

    }

}
