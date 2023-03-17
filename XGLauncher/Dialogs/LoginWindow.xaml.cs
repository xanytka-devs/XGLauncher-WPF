using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using XGL.Networking.Authencation;
using XGL.Networking.Database;
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
            ApplyLocalization(RegistrySLS.LoadString("Language"));
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object _sender, RoutedEventArgs _e) {
            Loaded -= LoginWindow_Loaded;
            CheckForLWButtons();
            if (!App.OnlineMode) {
                if (!Database.TryOpenConnection()) {
                    App.RunMySQLCommands = false;
                    ErrorBox box = new ErrorBox() {
                        ErrorTitle = warnings[0],
                        ErrorDescription = warnings[1],
                        ErrorImage = ErrorBox.ErrorBoxEType.Network,
                    };
                    //box.ErrorButton1 = new ErrorBox.ErrorBoxButton("Retry", false, true, (object sender, RoutedEventArgs e) => { Process.Start(Path.Combine(Environment.CurrentDirectory, "XGLauncher.exe")); box.Close(); });
                    //box.ErrorButton2 = new ErrorBox.ErrorBoxButton("Offline mode", true, false, (object _sender, RoutedEventArgs _e) => { box.Close(); });
                    box.Show();
                    Close();
                    return;
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
                    NextWindow(acc);
                    return;
                } else MessageBox.Show("This account isn't avaible currently. Please write to support@xan.ru if you have questions.");
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

        void TB_TextChanged(object sender, TextChangedEventArgs e) {



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
            if (nicknameTB.Text == "Not Set" || nicknameTB.Text.Contains(Path.GetInvalidPathChars().ToString())) {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[4];
                invalid = true;
            }
            if (nicknameTB.Text.ToLower() == "null") {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[4];
                invalid = true;
            }
            if (loginRTB.Text == "Not Set" || loginRTB.Text.Contains(Path.GetInvalidPathChars().ToString())) {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[4];
                invalid = true;
            }
            if (loginRTB.Text.ToLower() == "null") {
                nicknameWarn.Opacity = 100;
                nicknameWarn.Text = warnings[4];
                invalid = true;
            }
            if (passwordRTB.Text.ToLower() == "null") {
                passwordRWarn.Opacity = 100;
                passwordRWarn.Text = warnings[5];
                invalid = true;
            }
            if (emailTB.Text.ToLower() == "null") {
                emailWarn.Opacity = 100;
                emailWarn.Text = warnings[5];
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
                    RegistrySLS.Save("LoginData", nicknameTB.Text + ";" + _pass);
                    RegistrySLS.Save("LastID", Database.GetID(acc));
                    RegistrySLS.Save("Username", nicknameTB.Text);
                    RegistrySLS.Save("Description", "INS");
                    NextWindow(acc);
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
            if (Utils.NotEmptyAndNotNotSet(App.CurrentAccount.Password)) {
                //Save username and password to settings.
                App.AccountData = new string[] { acc.Login, acc.Password};
                App.CurrentAccount = new Account(App.AccountData[0], App.AccountData[1]);
                RegistrySLS.Save("LastID", Database.GetID(acc));
                Database.SetValue(Database.DBDataType.DT_ACTIVITY, 1);
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
                    VKAuthencator.Login();
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

        public void ReturnRequest(string c, WebResponse response) {
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

        public void ApplyLocalization(string localization) {

            switch (localization) {
                case "ru-RU":
                    loginRT.Text = "Логин";
                    loginT.Text = "Логин";
                    nicknameT.Text = "Ник";
                    emailT.Text = "Почта";
                    passwordRT.Text = "Пароль";
                    passwordT.Text = "Пароль";
                    RegisterBtn.Content = "Регистрация";
                    sw_login.Content = "Войти в аккаунт";
                    swith_to_reg.Content = "Регистрация";
                    LoginBtn.Content = "Войти в аккаунт";
                    warnings = new string[] {
                        "Ошибка сети", "Произошла ошибка при подключении к датабазе. Пожалуйста попробуйте позже.", "Нет логина",
                            "Нет пароля", "Запрещённый логин", "Запрещённый пароль", "Нет почты", "Неправильный пароль", 
                            "Неправильный логин", "Запрещённая почта", "Слишком длинный логин", "Слишком длинная почта", "Слишком длинный ник",
                            "Слишком длинный пароль", "Нет ника", "Слишком малая почта", "Слишком малый логин", "Слишком малый пароль", 
                            "Слишком малый ник"};
                    break;
                case "en-US":
                    loginRT.Text = "Login";
                    loginT.Text = "Login";
                    nicknameT.Text = "Nickname";
                    emailT.Text = "Email";
                    passwordRT.Text = "Password";
                    passwordT.Text = "Password";
                    RegisterBtn.Content = "Register";
                    sw_login.Content = "Login";
                    swith_to_reg.Content = "Register";
                    LoginBtn.Content = "Login";
                    warnings = new string[] {
                        "Network error", "Some error occured while trying to connect to database. Please try again later.", "No login",
                            "No password", "Forbidden login", "Forbidden password", "No email", "Wrong password", "Wrong login", "Forbidden email",
                            "Too long login", "Too long email", "Too long username", "Too long password", "No username", "Too short email", 
                            "Too short login", "Too short password", "Too short username"};
                    break;
                case "es":
                    loginRT.Text = "Inicio de sesión";
                    loginT.Text = "Inicio de sesión";
                    nicknameT.Text = "Título";
                    emailT.Text = "Correo";
                    passwordRT.Text = "Contraseña";
                    passwordT.Text = "Contraseña";
                    RegisterBtn.Content = "Registro";
                    sw_login.Content = "Iniciar sesión";
                    swith_to_reg.Content = "Registro";
                    LoginBtn.Content = "Iniciar sesión";
                    warnings = new string[] {
                        "Error de red", " se Produjo un error al conectarse a la base de datos. Por favor intente más tarde.", "Sin Inicio de sesión",
                            "Sin contraseña", "Inicio de sesión Prohibido", "contraseña Prohibida", "sin correo", "contraseña Incorrecta",
                            "Inicio de sesión incorrecto"," correo Prohibido"," Inicio de sesión Demasiado largo", "correo demasiado largo",
                            "apodo Demasiado largo", "Contraseña demasiado larga", "Sin rango", "Correo demasiado pequeño",
                            "Inicio de sesión Demasiado pequeño", "contraseña Demasiado pequeña", "apodo Demasiado pequeño"};
                    break;
                case "ru-IM":
                    loginRT.Text = "Логинъ";
                    loginT.Text = "Логинъ";
                    nicknameT.Text = "Званіе";
                    emailT.Text = "Почта";
                    passwordRT.Text = "Пароль";
                    passwordT.Text = "Пароль";
                    RegisterBtn.Content = "Регистрація";
                    sw_login.Content = "Войти въ аккаунтъ";
                    swith_to_reg.Content = "Регистрація";
                    LoginBtn.Content = "Войти въ аккаунтъ";
                    warnings = new string[] {
                        "Ошибка сѣти", "Произошла ошибка при подключеніи къ датабазе. Пожалуйста попробуйте позже.", "Нѣтъ ​логина​",
                            "Нѣтъ пароля", "Запрещенный ​логинъ​", "Запрещенный пароль", "Нѣтъ почты", "Неправильный пароль",
                            "Неправильный ​логинъ​", "Запрещенная почта", "Слишкомъ длинный ​логинъ​", "Слишкомъ длинная почта", "Слишкомъ длинный никъ",
                            "Слишкомъ длинный пароль", "Нѣтъ ника", "Слишкомъ малая почта", "Слишкомъ малый ​логинъ​", 
                        "Слишкомъ малый пароль", "Слишкомъ малый никъ"};
                    break;
            }

        }

    }

}
