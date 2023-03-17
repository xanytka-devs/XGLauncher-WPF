using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
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
using System.IO;
using XGL.Networking;
using XGL.SLS;
using static XGL.Networking.Database;

namespace XGL.Dev {

    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    
    public partial class LoginWindow : Window {

        string[] warnings;

        public LoginWindow() {
            InitializeComponent();
            ApplyLocalization(RegistrySLS.LoadXGLString("Language"));
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object _sender, RoutedEventArgs _e) {

            Loaded -= LoginWindow_Loaded;

            //Check for updates.
            CheckForUpdates();
            Account acc = App.CurrentAccount;
            if (AccountExisting(acc))
                NextWindow(acc);

        }

        void CheckForUpdates() {
            string output = string.Empty;
            MySqlCommand command = new MySqlCommand($"SELECT `latest` FROM `applications` WHERE `id` = 2", Connection);
            try {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) output = dr.GetString("latest");
                dr.Close();
                CloseConnection();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally { CloseConnection(); }
            if (output.Split('{')[0] != App.CurrentVersion) {
                Process.Start(Path.Combine(App.CurrentFolder, "XGLauncher Updater.exe"), "/devtools");
                Close();
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e) {

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

            UIStabilize();

            string _pass = Base64.ToBase64String(Encoding.ASCII.GetBytes(passwordTB.Text));

            Account acc = new Account(loginTB.Text, _pass);

            if (AccountExisting(acc)) {
                if (AccountValid(acc)) {
                    RegistrySLS.Save("PublisherLoginData", loginTB.Text + ";" + _pass);
                    RegistrySLS.Save("Description", Database.GetValue(acc, "description"));
                    RegistrySLS.Save("LastID", Database.GetID(acc));
                    App.CurrentAccount = acc;
                    App.AccountData = new string[] { acc.Login, acc.Password };
                    NextWindow(acc);
                    return;
                }
                else MessageBox.Show("This account isn't avaible currently. Please write to support@xan.ru if you have questions.");
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

        void UIStabilize() {

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

        void PassToggle_U(object sender, RoutedEventArgs e) {
            passwordTBH.Visibility = Visibility.Visible;
            passwordTB.Visibility = Visibility.Collapsed;
            passwordTB.Text = passwordTBH.Password;
        }

        void PassToggle_C(object sender, RoutedEventArgs e) {
            passwordTBH.Visibility = Visibility.Collapsed;
            passwordTB.Visibility = Visibility.Visible;
            passwordTB.Text = passwordTBH.Password;
        }

        void NotifyRegister(object sender, RoutedEventArgs e) => Process.Start("https://xanytka.ru/sl/xglpr");

        void NextWindow(Account acc) {

            //Hides this window.
            Hide();

            if (Utils.NotEmptyAndNotNotSet(App.CurrentAccount.Password)) {
                //Save username and password to settings.
                App.AccountData = new string[] { acc.Login, acc.Password };
                App.CurrentAccount = new Account(App.AccountData[0], App.AccountData[1]);
                ChangeData(DBDataType.DT_ACTIVITY, 1);
            }

            //Opens the launcher.
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            //Closes this window.
            Close();

        }

        public void ApplyLocalization(string localization) {

            switch (localization) {
                case "ru-RU":
                    loginT.Text = "Логин";
                    passwordT.Text = "Пароль";
                    LoginBtn.Content = "Войти в аккаунт";
                    notifyRegister.Content = "Регистрация";
                    warnings = new string[] {
                        "Ошибка сети", "Произошла ошибка при подключении к датабазе. Пожалуйста попробуйте позже.", "Нет логина",
                            "Нет пароля", "Запрещённый логин", "Запрещённый пароль", "Нет почты", "Неправильный пароль",
                            "Неправильный логин", "Запрещённая почта", "Слишком длинный логин", "Слишком длинная почта", "Слишком длинный ник",
                            "Слишком длинный пароль", "Нет ника"};
                    break;
                case "en-US":
                    loginT.Text = "Login";
                    passwordT.Text = "Password";
                    LoginBtn.Content = "Login";
                    notifyRegister.Content = "Register";
                    warnings = new string[] {
                        "Network error", "Some error occured while trying to connect to database. Please try again later.", "No login",
                            "No password", "Forbidden login", "Forbidden password", "No email", "Wrong password", "Wrong login", "Forbidden email",
                            "Too long login", "Too long email", "Too long username", "Too long password", "No username"};
                    break;
                case "es":
                    loginT.Text = "Inicio de sesión";
                    passwordT.Text = "Contraseña";
                    LoginBtn.Content = "Iniciar sesión";
                    notifyRegister.Content = "Registro";
                    warnings = new string[] {
                        "Error de red", " se Produjo un error al conectarse a la base de datos. Por favor intente más tarde.", "Sin Inicio de sesión",
                            "Sin contraseña", "Inicio de sesión Prohibido", "contraseña Prohibida", "sin correo", "contraseña Incorrecta",
                            "Inicio de sesión incorrecto"," correo Prohibido"," Inicio de sesión Demasiado largo", "correo demasiado largo",
                            "apodo Demasiado largo", "Contraseña demasiado larga", "Sin rango"};
                    break;
                case "ru-IM":
                    loginT.Text = "Логинъ";
                    passwordT.Text = "Пароль";
                    LoginBtn.Content = "Войти въ аккаунтъ";
                    notifyRegister.Content = "Регистрація";
                    warnings = new string[] {
                        "Ошибка сѣти", "Произошла ошибка при подключеніи къ датабазе. Пожалуйста попробуйте позже.", "Нѣтъ ​логина​",
                            "Нѣтъ пароля", "Запрещенный ​логинъ​", "Запрещенный пароль", "Нѣтъ почты", "Неправильный пароль",
                            "Неправильный ​логинъ​", "Запрещенная почта", "Слишкомъ длинный ​логинъ​", "Слишкомъ длинная почта", "Слишкомъ длинный никъ",
                            "Слишкомъ длинный пароль", "Нѣтъ ника"};
                    break;
            }

        }

    }
}
