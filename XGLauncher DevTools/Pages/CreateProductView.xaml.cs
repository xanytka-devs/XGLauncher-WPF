using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using static XGL.Networking.Database;
using XGL.Networking;
using XGL.SLS;

namespace XGL.Dev.Pages {

    /// <summary>
    /// Логика взаимодействия для CreateProductView.xaml
    /// </summary>
    
    public partial class CreateProductView : UserControl {

        public CreateProductView() {
            InitializeComponent();
        }

        bool inv = false;
        void Create(object sender, RoutedEventArgs e) {
            if (!App.RunMySQLCommands) return;
            RB();
            if (string.IsNullOrEmpty(TB_name.Text)) SRB(T_name);
            if (string.IsNullOrEmpty(TB_desc.Text)) SRB(T_desc);
            if (string.IsNullOrEmpty(TB_genr.Text)) SRB(T_genr);
            if (string.IsNullOrEmpty(TB_urll.Text)) SRB(T_urll);
            if (string.IsNullOrEmpty(TB_icon.Text)) SRB(T_icon);
            if (string.IsNullOrEmpty(TB_scsh.Text)) SRB(T_scsh);
            if (string.IsNullOrEmpty(TB_pric.Text)) SRB(T_pric);
            if (string.IsNullOrEmpty(TB_vers.Text)) SRB(T_vers);
            if (inv) return;
            MySqlCommand command = new MySqlCommand(
                $"INSERT INTO `xgl_products`(`name`, `publisherID`, `genres`, `storeBanner`, `storeMedia`, `price`, `description`, `latestDownloadLinks`) VALUES ('{TB_name.Text}', {RegistrySLS.LoadString("LastID")}, '{TB_genr.Text}', '{TB_icon.Text}', '{TB_scsh.Text}', '{TB_pric.Text}', '{TB_desc.Text}', '{TB_vers.Text + "{" + TB_urll.Text}')"
                , Connection);
            //Start the connection.
            OpenConnection();
            //Execute the data query.
            if (command.ExecuteNonQuery() == 1) {
                //Close the connection.
                CloseConnection();
            }
            string vals = Database.GetValue(App.CurrentAccount, "productIDs").ToString();
            string appid = string.Empty;
            //Initialize components
            command = new MySqlCommand($"SELECT `id` FROM `xgl_products` WHERE `name` = @nam and `publisherID` = @pid", Connection);
            //Add parameters
            command.Parameters.Add("@nam", MySqlDbType.VarChar).Value = TB_name.Text;
            command.Parameters.Add("@pid", MySqlDbType.VarChar).Value = RegistrySLS.LoadString("LastID");
            //Check if user exists or not.
            try {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) {
                    appid = dr.GetInt64("id").ToString();
                }
                dr.Close();
                CloseConnection();
            }
            catch (Exception ex) {
                //TODO: Implement custom dialog system.
                MessageBox.Show(ex.Message);
            }
            ChangeData(DBDataType.DT_PRODUCTS, vals + appid + ";");
            command = new MySqlCommand(
                $"UPDATE `xgl_products` SET `storeBanner`='{appid + "_storebanner01{" + TB_icon}',`storeMedia`='{appid + "_storeimage01{" + TB_scsh}' WHERE `name` = @nam and `publisherID` = @pid"
                , Connection);
            //Add parameters
            command.Parameters.Add("@nam", MySqlDbType.VarChar).Value = TB_name.Text;
            command.Parameters.Add("@pid", MySqlDbType.VarChar).Value = RegistrySLS.LoadString("LastID");
            //Start the connection.
            OpenConnection();
            //Execute the data query.
            if (command.ExecuteNonQuery() == 1) {
                //Close the connection.
                CloseConnection();
            }
            MainWindow.Instance.Reload();
        }

        void SRB(TextBlock tb) { tb.Foreground = new SolidColorBrush(Colors.Red); inv = true; }
        void RB() {
            T_name.Foreground = new SolidColorBrush(Colors.Red);
            T_desc.Foreground = new SolidColorBrush(Colors.Red);
            T_genr.Foreground = new SolidColorBrush(Colors.Red);
            T_urll.Foreground = new SolidColorBrush(Colors.Red);
            T_icon.Foreground = new SolidColorBrush(Colors.Red);
            T_scsh.Foreground = new SolidColorBrush(Colors.Red);
            T_pric.Foreground = new SolidColorBrush(Colors.Red);
            T_vers.Foreground = new SolidColorBrush(Colors.Red);
            inv = false;
        }

    }
}
