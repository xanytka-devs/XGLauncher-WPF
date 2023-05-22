using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XGL.Networking.Authencation;
using XGL.Networking;
using XGL.SLS;

namespace XGL.Dialogs.Login {

    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>

    public partial class LoginWindow : Window {

        public static LoginWindow Instance { get; private set; }
        public WebResponse response;
        string[] warnings;

        public LoginWindow() {
            InitializeComponent();
            Instance = this;
            ApplyLocalization();
            Loaded += LoginWindow_Loaded;
        }

        private async void LoginWindow_Loaded(object _sender, RoutedEventArgs _e) {
            Loaded -= LoginWindow_Loaded;
            CheckForLWButtons();
            if (!App.LoginDataNotSaved) {
                if (!Database.TryOpenConnection()) {
                    App.RunMySQLCommands = false;
                    while (!LocalizationManager.I.LocalLoaded()) await Task.Delay(25);
                    ErrorBox box = new ErrorBox {
                        ErrorTitle = warnings[0],
                        ErrorDescription = warnings[1],
                        ErrorImage = ErrorBox.ErrorBoxEType.Network,
                    };
                    box.ErrorButton1 = new ErrorBox.ErrorBoxButton("Retry", false, true,
                        (object sender, RoutedEventArgs e) => {
                            Process.Start(Path.Combine(Environment.CurrentDirectory, "XGLauncher.exe"));
                            box.Close();
                        }
                    );
                    box.ErrorButton2 = new ErrorBox.ErrorBoxButton("Offline mode", true, false, (object sender, RoutedEventArgs e) => {
                        ProcessStartInfo xglom = new ProcessStartInfo {
                            UseShellExecute = true,
                            FileName = Path.Combine(Environment.CurrentDirectory, "XGLauncher.exe"),
                            Arguments = "-offline"
                        };
                        box.Close();
                    });
                    box.Show();
                    Close();
                    return;
                }
            } else {
                if (!Database.AccountEmailVerified(App.CurrentAccount)) {
                    LoginPage.Visibility = Visibility.Collapsed;
                    EmaiNotVerPage.Visibility = Visibility.Visible;
                }
            }
        }

        void CheckForLWButtons() {
            if (RegistrySLS.LoadBool("LoginAuth", false)) return;
            lb_google.Visibility = Visibility.Collapsed;
            lb_vk.Visibility = Visibility.Collapsed;
            lb_tg.Visibility = Visibility.Collapsed;
            lb_whatsapp.Visibility = Visibility.Collapsed;
            lb_github.Visibility = Visibility.Collapsed;
            swith_to_reg.Margin = new Thickness(0, 0, 0, 60);
        }

        void LoginBtn_Click(object sender, RoutedEventArgs e) {
            passwordTB.Text = passwordTBH.Password;
            passwordWarn.Opacity = 0;
            loginWarn.Opacity = 0;
            bool isInvalid = false;
            if (string.IsNullOrEmpty(loginTB.Text)) {
                loginWarn.Opacity = 100;
                loginWarn.Text = warnings[2];
                isInvalid = true;
            } 
            if (string.IsNullOrEmpty(passwordTB.Text)) {
                passwordWarn.Opacity = 100;
                passwordWarn.Text = warnings[3];
                isInvalid = true;
            }
            if (loginTB.Text.ToLower() == "null") {
                loginWarn.Opacity = 100;
                loginWarn.Text = warnings[4];
                isInvalid = true;
            }
            if (passwordTB.Text.ToLower() == "null") {
                passwordWarn.Opacity = 100;
                passwordWarn.Text = warnings[5];
                isInvalid = true;
            }

            if (isInvalid) return;

            UIStabilize(1);

            string _pass = Base64.ToBase64String(Encoding.ASCII.GetBytes(passwordTB.Text));

            Account acc = new Account(loginTB.Text, _pass);

            if (Database.AccountExisting(acc)) {
                if (Database.AccountValid(acc)) {
                    RegistrySLS.Save("LoginData", loginTB.Text + ";" + _pass);
                    RegistrySLS.Save("LastID", Database.GetID(acc));
                    RegistrySLS.Save("Username", Database.GetValue(acc, "username"));
                    RegistrySLS.Save("Description", Database.GetValue(acc, "description"));
                    App.CurrentAccount = acc;
                    App.AccountData = new string[] { acc.Login, acc.Password };
                    if (!Database.AccountEmailVerified(App.CurrentAccount)) {
                        LoginPage.Visibility = Visibility.Collapsed;
                        EmaiNotVerPage.Visibility = Visibility.Visible;
                        return;
                    }
                    NextWindow(acc);
                    return;
                } else
                    MessageBox.Show("This account isn't avaible currently. Please write to support@xanytka.ru if you have questions.");
            }

            LoginBtn.IsEnabled = true;
            loginTB.IsEnabled = true;
            passwordTB.IsEnabled = true;

            LoginBtn.Opacity = 1;
            loginT.Opacity = 1;
            loginTB.Opacity = 1;
            passwordT.Opacity = 1;
            passwordTB.Opacity = 1;
            logo.Opacity = 1;

            passwordWarn.Opacity = 100;
            loginWarn.Opacity = 100;
            passwordWarn.Text = warnings[7];
            loginWarn.Text = warnings[8];

        }

        void RegBtn_Click(object sender, RoutedEventArgs e) {

            RegisterPage.Visibility = Visibility.Visible;
            LoginPage.Visibility = Visibility.Collapsed;

        }

        void LogBtn_Click(object sender, RoutedEventArgs e) {

            RegisterPage.Visibility = Visibility.Collapsed;
            LoginPage.Visibility = Visibility.Visible;

        }

        void RegisterBtn_Click(object sender, RoutedEventArgs e) {

            bool invalid = false;

            nicknameWarn.Opacity = 0;
            loginRWarn.Opacity = 0;
            passwordRWarn.Opacity = 0;
            emailWarn.Opacity = 0;

            if (string.IsNullOrEmpty(nicknameTB.Text) & string.IsNullOrEmpty(passwordRTB.Text) 
                & string.IsNullOrEmpty(emailTB.Text) & string.IsNullOrEmpty(loginRTB.Text)) {
                loginRWarn.Opacity = 100;
                loginRWarn.Text = warnings[2];
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[14];
                passwordRWarn.Opacity = 100;
                passwordRWarn.Text = warnings[3];
                emailWarn.Opacity = 100;
                emailWarn.Text = warnings[6];
                return;
            }
            if (emailTB.Text.Length > 30) { emailWarn.Opacity = 100; emailWarn.Text = warnings[11]; invalid = true; }
            if (loginRTB.Text.Length > 30) { loginRWarn.Opacity = 100; loginRWarn.Text = warnings[10]; invalid = true; }
            if (passwordRTB.Text.Length > 30) { passwordRWarn.Opacity = 100; passwordRWarn.Text = warnings[13]; invalid = true; }
            if (nicknameTB.Text.Length > 25) { nicknameWarn.Opacity = 100; nicknameWarn.Text = warnings[12]; invalid = true; }
            if (emailTB.Text.Length < 8) { emailWarn.Opacity = 100; emailWarn.Text = warnings[15]; invalid = true; }
            if (loginRTB.Text.Length < 5) { loginRWarn.Opacity = 100; loginRWarn.Text = warnings[16]; invalid = true; }
            if (passwordRTB.Text.Length < 5) { passwordRWarn.Opacity = 100; passwordRWarn.Text = warnings[17]; invalid = true; }
            if (nicknameTB.Text.Length < 0) { nicknameWarn.Opacity = 100; nicknameWarn.Text = warnings[18]; invalid = true; }
            if (string.IsNullOrEmpty(nicknameTB.Text)) {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[14];
                invalid = true;
            }
            if (string.IsNullOrEmpty(passwordRTB.Text)) {
                passwordRWarn.Opacity = 100;
                passwordRWarn.Text = warnings[3];
                invalid = true;
            }
            if (string.IsNullOrEmpty(emailTB.Text)) {
                emailWarn.Opacity = 100;
                emailWarn.Text = warnings[6];
                invalid = true;
            }
            if (nicknameTB.Text == "INS" || nicknameTB.Text.Contains(Path.GetInvalidPathChars().ToString())) {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[19];
                invalid = true;
            }
            if (loginRTB.Text == "INS" || loginRTB.Text.Contains(Path.GetInvalidPathChars().ToString())) {
                loginRWarn.Opacity = 100;
                loginRWarn.Text = warnings[4];
                invalid = true;
            }
            if (passwordRTB.Text == "INS") {
                passwordRWarn.Opacity = 100;
                passwordRWarn.Text = warnings[5];
                invalid = true;
            }
            if (emailTB.Text == "INS") {
                emailWarn.Opacity = 100;
                emailWarn.Text = warnings[4];
                invalid = true;
            }
            if (!string.IsNullOrEmpty(emailTB.Text)) {
                if (!emailTB.Text.Contains('@') ||
                    !emailTB.Text.Contains('.')) {
                    emailWarn.Opacity = 100;
                    emailWarn.Text = warnings[9];
                    invalid = true;
                }
            }

            if (invalid) { return; }
            UIStabilize();
            string _pass = Base64.ToBase64String(Encoding.ASCII.GetBytes(passwordRTB.Text));
            Account acc = new Account(loginRTB.Text, _pass);

            if (!Database.AccountExisting(acc)) {
                if (Database.CreateAccount(acc, nicknameTB.Text, emailTB.Text)) {
                    RegistrySLS.Save("LoginData", loginRTB.Text + ";" + _pass);
                    RegistrySLS.Save("LastID", Database.GetID(acc));
                    RegistrySLS.Save("Username", nicknameTB.Text);
                    RegistrySLS.Save("Description", "INS");
                    App.CurrentAccount = acc;
                    App.AccountData = new string[] { acc.Login, acc.Password };
                    App.IsFirstRun = true;
                    if (EmailAuthencator.SendVerificationMsg(emailTB.Text)) {
                        RegisterPage.Visibility = Visibility.Collapsed;
                        EmailVerificationPage.Visibility = Visibility.Visible;
                    } else {
                        emailWarn.Opacity = 100;
                        emailWarn.Text = warnings[9];
                    }
                }
            } else {

                LoginBtn.IsEnabled = true;
                loginTB.IsEnabled = true;
                passwordTB.IsEnabled = true;

                LoginBtn.Opacity = 1;
                loginT.Opacity = 1;
                loginTB.Opacity = 1;
                passwordT.Opacity = 1;
                passwordTB.Opacity = 1;
                logo.Opacity = 1;

                passwordWarn.Opacity = 100;
                passwordWarn.Text = warnings[7];
                loginWarn.Opacity = 100;
                loginWarn.Text = warnings[8];

            }

        }

        void NextWindow(Account acc) {
            //Hides this window.
            Hide();
            if (Utils.I.NotEmptyAndNotINS(App.CurrentAccount.Password)) {
                //Save username and password to settings.
                App.AccountData = new string[] { acc.Login, acc.Password};
                App.CurrentAccount = new Account(App.AccountData[0], App.AccountData[1]);
                RegistrySLS.Save("LastID", Database.GetID(acc));
                Database.SetValue(Database.DBDataType.DT_ACTIVITY, 1);
                if(!string.IsNullOrEmpty(EmailAuthencator.VerificationCode)) 
                    Database.SetValue(Database.DBDataType.DT_EMAILCODE, "1");
            }
            //Opens the launcher.
            MainWindow.Show(this);
        }

        void UIStabilize(int uiIndex = 0) {

            if (uiIndex == 0) {
                nicknameWarn.Opacity = 0;
                passwordRWarn.Opacity = 0;
                emailWarn.Opacity = 0;

                RegisterBtn.IsEnabled = false;
                nicknameTB.IsEnabled = false;
                passwordRTB.IsEnabled = false;

                RegisterBtn.Opacity = 0.25;
                nicknameT.Opacity = 0.25;
                nicknameTB.Opacity = 0.25;
                passwordRT.Opacity = 0.25;
                passwordRTB.Opacity = 0.25;
                emailT.Opacity = 0.25;
                emailTB.Opacity = 0.25;
            } else {
                loginWarn.Opacity = 0;
                passwordWarn.Opacity = 0;

                LoginBtn.IsEnabled = false;
                loginTB.IsEnabled = false;
                passwordTB.IsEnabled = false;

                LoginBtn.Opacity = 0.25;
                loginT.Opacity = 0.25;
                loginTB.Opacity = 0.25;
                passwordT.Opacity = 0.25;
                passwordTB.Opacity = 0.25;
                logo.Opacity = 0.125;
            }

        }

        void Auth_btn_Click(object sender, RoutedEventArgs e) {

            switch ((sender as FrameworkElement).Name) {
                case "lb_google":
                    GoogleAuthencator.Login();
                    break;
                case "lb_vk":
                    MessageBox.Show("Oh no, looks like this authencation method isn't implemented", "XGLauncher");
                    break;
                case "lb_tg":
                    MessageBox.Show("Oh no, looks like this authencation method isn't implemented", "XGLauncher");
                    break;
                case "lb_whatsapp":
                    MessageBox.Show("Oh no, looks like this authencation method isn't implemented", "XGLauncher");
                    break;
                case "lb_github":
                    MessageBox.Show("Oh no, looks like this authencation method isn't implemented", "XGLauncher");
                    break;
            }

        }

        public void ReturnRequest(string c/*, WebResponse response*/) {
            Activate();
            string nl = string.Empty;
            string np = string.Empty;
            switch (c) {
                case "gg":
                    break;
                case "yt":
                    break;
                case "vk":
                    break;
                case "tg":
                    break;
                case "ds":
                    break;
            }
            loginTB.Text = nl;
            passwordTB.Text = np;
            UIStabilize();
        }

        void PassToggle_U(object sender, RoutedEventArgs e) {
            passwordTBH.Visibility = Visibility.Visible;
            passwordTB.Visibility = Visibility.Collapsed;
            passwordTB.Text = passwordTBH.Password;
        }

        void PassToggle_C(object sender, RoutedEventArgs e) {
            passwordTBH.Visibility= Visibility.Collapsed;
            passwordTB.Visibility= Visibility.Visible;
            passwordTB.Text = passwordTBH.Password;
        }

        void PassChanged_T(object sender, TextChangedEventArgs e) => passwordTBH.Password = passwordTB.Text;

        public async void ApplyLocalization() {

            LocalizationManager l = LocalizationManager.I;

            while (!l.LocalLoaded()) await Task.Delay(25);

            loginT.Text = loginRT.Text = l.dictionary["gn.log"];
            swith_to_reset_pass.Content = l.dictionary["gn.restore"];
            nicknameT.Text = l.dictionary["lw.nick"];
            passwordRT.Text = passwordT.Text = l.dictionary["gn.pass"];
            RegisterBtn.Content = swith_to_reg.Content = l.dictionary["lw.reg"];
            LoginBtn.Content = sw_login.Content = l.dictionary["lw.log"];
            warnings = l.dictionary["lw.warns"].Split('|');
            changeEmailT.Text = l.dictionary["lw.chemail.t"];
            changeEmailD.Text = l.dictionary["lw.chemail.d"];
            emailChangeB.Content = verificationCodeB.Content =
            wrongDataB.Content = resetPassVerCodeB.Content = resetPassB.Content
            = l.dictionary["gn.continue"];
            sw_veremail.Content = sw_verResetPass.Content = sw_resetPass.Content
            = l.dictionary["gn.back"];
            emailVerT.Text = emailVerCT.Text = l.dictionary["lw.verify.t"];
            emailVerD.Text = l.dictionary["lw.verify.d"];
            emailVerCD.Text = l.dictionary["lw.verify.c.d"];
            warnEmailVer.Text = resetPassWarn.Text = l.dictionary["lw.verify.c.warn"];
            sw_email.Content = l.dictionary["lw.verify.c.nc"];
            resetPassT.Text = resetPassVerCT.Text = l.dictionary["lw.resetpass.t"];
            resetPassD.Text = l.dictionary["lw.resetpass.d"];
            resetPassVerCD.Text = l.dictionary["lw.resetpass.c.d"];
            rpNewPassT.Text = l.dictionary["lw.newpass"];
            rpVCT.Text = l.dictionary["lw.vercode"];
            emailT.Text = rpEmailT.Text = l.dictionary["gn.mail"];

        }

        void EmailVerificationTB_Lost(object sender, RoutedEventArgs e) {
            if(string.IsNullOrEmpty(verificationCodeTB.Text)) {
                verificationCodeTB.Text = "00000";
                verificationCodeTB.Foreground = new SolidColorBrush(Colors.Gray);
            }
            if (string.IsNullOrEmpty(resetPassVerCodeTB.Text)) {
                resetPassVerCodeTB.Text = "00000";
                resetPassVerCodeTB.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        void EmailVerificationTB_Gain(object sender, RoutedEventArgs e) {
            if (verificationCodeTB.Text == "00000")
                verificationCodeTB.Text = string.Empty;
            verificationCodeTB.Foreground = new SolidColorBrush(Colors.White);
            if (resetPassVerCodeTB.Text == "00000")
                resetPassVerCodeTB.Text = string.Empty;
            resetPassVerCodeTB.Foreground = new SolidColorBrush(Colors.White);
        }
        void WrongEmailBtn_Click(object sender, RoutedEventArgs e) {
            EmailVerificationPage.Visibility = Visibility.Collapsed;
            EmailWrongPage.Visibility = Visibility.Visible;
            sw_veremail.Visibility = Visibility.Visible;
        }

        void CheckVerCode(object sender, RoutedEventArgs e) {
            if (verificationCodeTB.Text == EmailAuthencator.VerificationCode) {
                EmailAuthencator.SendRegOverMsg(Database.GetValue(App.CurrentAccount, "email").ToString());
                NextWindow(App.CurrentAccount);
            } else warnEmailVer.Visibility = Visibility.Visible;
        }

        void VerifyEmail(object sender, RoutedEventArgs e) {
            if (EmailAuthencator.SendVerificationMsg(Database.GetValue(App.CurrentAccount, "email").ToString())) {
                EmailWrongPage.Visibility = Visibility.Collapsed;
                EmaiNotVerPage.Visibility = Visibility.Collapsed;
                EmailVerificationPage.Visibility = Visibility.Visible;
            } else {
                EmaiNotVerPage.Visibility = Visibility.Collapsed;
                EmailWrongPage.Visibility = Visibility.Visible;
                sw_veremail.Visibility = Visibility.Collapsed;
            }
        }

        void ChangeEmail(object sender, RoutedEventArgs e) {
            warnEmailChange.Visibility = Visibility.Collapsed;
            bool invalid = false;
            if (newEmailTB.Text.Length < 8) { warnEmailChange.Visibility = Visibility.Visible; warnEmailChange.Text = warnings[15]; invalid = true; }
            if (string.IsNullOrEmpty(newEmailTB.Text)) {
                warnEmailChange.Visibility = Visibility.Visible;
                warnEmailChange.Text = warnings[6];
                invalid = true;
            }
            if (newEmailTB.Text == "INS") {
                warnEmailChange.Visibility = Visibility.Visible;
                warnEmailChange.Text = warnings[4];
                invalid = true;
            }
            if (!string.IsNullOrEmpty(newEmailTB.Text)) {
                if (!newEmailTB.Text.Contains('@') ||
                    !newEmailTB.Text.Contains('.')) {
                    warnEmailChange.Visibility = Visibility.Visible;
                    warnEmailChange.Text = warnings[9];
                    invalid = true;
                }
            }
            if (invalid) return;
            Database.SetValue(Database.DBDataType.DT_EMAIL, newEmailTB.Text);
            VerifyEmail(sender, e);
        }

        void BackNewEmailBtn_Click(object sender, RoutedEventArgs e) {
            EmaiNotVerPage.Visibility = Visibility.Collapsed;
            EmailVerificationPage.Visibility = Visibility.Visible;
        }

        void ResPassBtn_Click(object sender, RoutedEventArgs e) {
            LoginPage.Visibility = Visibility.Collapsed;
            PasswordResetPage.Visibility = Visibility.Visible;
        }

        void BackPassResetBtn_Click(object sender, RoutedEventArgs e) {
            LoginPage.Visibility = Visibility.Visible;
            PasswordResetPage.Visibility = Visibility.Collapsed;
            PasswordResetVerificationPage.Visibility = Visibility.Collapsed;
        }

        void ResetPassword(object sender, RoutedEventArgs e) {
            rpEmailWarn.Visibility = Visibility.Collapsed;
            bool invalid = false;
            if (rpEmailTB.Text.Length < 8) { rpEmailWarn.Visibility = Visibility.Visible; rpEmailWarn.Text = warnings[15]; invalid = true; }
            if (string.IsNullOrEmpty(rpEmailTB.Text)) {
                rpEmailWarn.Visibility = Visibility.Visible;
                rpEmailWarn.Text = warnings[6];
                invalid = true;
            }
            if (rpEmailTB.Text == "INS") {
                rpEmailWarn.Visibility = Visibility.Visible;
                rpEmailWarn.Text = warnings[4];
                invalid = true;
            }
            if (!string.IsNullOrEmpty(rpEmailTB.Text)) {
                if (!rpEmailTB.Text.Contains('@') ||
                    !rpEmailTB.Text.Contains('.')) {
                    rpEmailWarn.Visibility = Visibility.Visible;
                    rpEmailWarn.Text = warnings[9];
                    invalid = true;
                }
            }
            if (invalid) return;
            RegistrySLS.Save("LastID", Database.GetValue("email", rpEmailTB.Text, "id"));
            EmailAuthencator.SendResetPasswordMsg(rpEmailTB.Text);
            PasswordResetPage.Visibility = Visibility.Collapsed;
            PasswordResetVerificationPage.Visibility = Visibility.Visible;
        }

        void CheckRPVerCode(object sender, RoutedEventArgs e) {
            if (resetPassVerCodeTB.Text == EmailAuthencator.VerificationCode) {
                Database.SetValue(Database.DBDataType.DT_PASSWORD, Base64.ToBase64String(
                    Encoding.UTF8.GetBytes(rpNewPassTB.Text)));
                BackPassResetBtn_Click(sender, e);
            } else resetPassWarn.Visibility = Visibility.Visible;
        }
    }

}
