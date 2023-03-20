using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using XGL.SLS;
using XGL.Dev;

namespace XGL.Networking {

    public class Account {

        public string Login { get; private set; }
        public string Password { get; private set; }

        public Account(string login, string password) {
            Login = login;
            Password = password;
        }

    }

    internal class ExtendedAccount {

        public string Login { get; private set; }
        public string Password { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public ExtendedAccount(string login, string password, string fname, string lname) {
            Login = login;
            Password = password;
            FirstName = fname;
            LastName = lname;
        }

    }

    internal class Database {

        /// <summary>
        /// Shows data type to get/set in Database.
        /// </summary>
        public enum DBDataType {
            DT_NAME,
            DT_ICON,
            DT_EMAIL,
            DT_PASSWORD,
            DT_ACTIVITY,
            DT_DESCRIPTION,
            DT_PRODUCTS,
            DT_PERSONALIZATIONVALUES,
            DT_COMMANDIDS,
        }

        public static string[] ProductIDs;
        public static string[] CommandIDs;
        internal static readonly MySqlConnection Connection = new MySqlConnection(App.DBConnectorData);

        /// <summary>
        /// Opens new Database connection.
        /// </summary>
        internal static bool OpenConnection() {
            if (!App.RunMySQLCommands) return false;
            try {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        /// <summary>
        /// Closes open Database connection.
        /// </summary>
        internal static bool CloseConnection() {
            if (!App.RunMySQLCommands) return false;
            try {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Checks if connection is possible.
        /// </summary>
        /// <returns>true=possible;false=isn't possible</returns>
        public static bool TryOpenConnection() {
            if (!App.RunMySQLCommands) return false;
            return OpenConnection() && CloseConnection();
        }

        /// <summary>
        /// Checks if account exists.
        /// </summary>
        /// <param name="account">Account that will be checked for existance.</param>
        /// <returns>true=exists;false=doesn't exist</returns>
        public static bool AccountExisting(Account account) {
            if (!App.RunMySQLCommands) return false;
            //Initialize components
            string log = string.Empty;
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand("SELECT * FROM `xgl_publishers` WHERE `name` = @log and `password` = @pass", Connection);
            //Add parameters
            command.Parameters.Add("@log", MySqlDbType.VarChar).Value = account.Login;
            command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = account.Password;
            //Initialize adapter
            adapter.SelectCommand = command;
            adapter.Fill(table);
            //Check if user exists or not.
            if (table.Rows.Count > 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if account is valid.
        /// </summary>
        /// <param name="account">Account that will be checked for validation.</param>
        /// <returns>true=valid;false=invalid</returns>
        public static bool AccountValid(Account account) {
            if (!App.RunMySQLCommands) return false;
            int status = int.Parse(GetValue(account, "activity").ToString());
            if (status != 9) return true;
            else return false;
        }

        /// <summary>
        /// Gets passed user's ID.
        /// </summary>
        /// <param name="account">User to search for.</param>
        /// <returns>string(string(#0000);int(0))</returns>
        public static string GetID(Account account) {
            if (!App.RunMySQLCommands) return null;
            if (AccountExisting(account)) {
                //Initialize components
                long output = 0;
                MySqlCommand command = new MySqlCommand("SELECT * FROM `xgl_publishers` WHERE `name` = @log and `password` = @pass", Connection);
                //Add parameters
                command.Parameters.Add("@log", MySqlDbType.VarChar).Value = account.Login;
                command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = account.Password;
                //Check if user exists or not.
                try {
                    OpenConnection();
                    MySqlDataReader dr;
                    dr = command.ExecuteReader();
                    while (dr.Read()) {
                        output = dr.GetInt64("id");
                    }
                    dr.Close();
                    CloseConnection();
                    return output.ToString();
                }
                catch (Exception ex) {
                    //TODO: Implement custom dialog system.
                    MessageBox.Show(ex.Message);
                    return "0";
                }
            }
            return "0";
        }

        /// <summary>
        /// Saves all data about command IDs.
        /// </summary>
        public static void GetProductIDs() {
            if(!App.RunMySQLCommands) return;
            if (App.CurrentAccount.Login != "INS" && App.CurrentAccount.Login.ToLower() != "not set") {
                CommandIDs = GetValue(App.CurrentAccount, "productIDs")
                        .ToString().Split(';');
                if (CommandIDs.Length > 1)
                    CommandIDs = CommandIDs.Where((val, idx) => idx != CommandIDs.Length - 1).ToArray();
            }
        }

        /// <summary>
        /// Saves all data about products.
        /// </summary>
        public static void GetCommandIDs() {
            if (!App.RunMySQLCommands) return;
            if (App.CurrentAccount.Login != "INS" && App.CurrentAccount.Login.ToLower() != "not set") {
                ProductIDs = GetValue(App.CurrentAccount, "productIDs")
                        .ToString().Split(';');
                if (ProductIDs.Length > 1)
                    ProductIDs = ProductIDs.Where((val, idx) => idx != ProductIDs.Length - 1).ToArray();
            }
        }

        /// <summary>
        /// Gets all ids of products on account in string like "1, 2'.
        /// </summary>
        /// <returns>String like "1,2"</returns>
        public static string GetAppIDs() {
            string Out = string.Empty;
            if (!string.IsNullOrEmpty(ProductIDs[0])) {
                for (int i = 0; i < ProductIDs.Length; i++) {
                    if (i != ProductIDs.Length - 1) Out += ProductIDs[i] + ", ";
                    else Out += ProductIDs[i];
                }
            }
            else Out = ProductIDs[0];
            return Out;
        }

        /// <summary>
        /// Gets value from row.
        /// </summary>
        /// <param name="acc">Account, in which row data is located.</param>
        /// <param name="column">Column in which data is located.</param>
        /// <returns>object(value from Database)</returns>
        public static object GetValue(Account acc, string column) {
            if (!App.RunMySQLCommands) return null;
            if (AccountExisting(acc)) {
                //Initialize components
                object output = 0;
                MySqlCommand command = new MySqlCommand($"SELECT `{column.ToLower()}` FROM `xgl_publishers` WHERE `name` = @log and `password` = @pass", Connection);
                //Add parameters
                command.Parameters.Add("@log", MySqlDbType.VarChar).Value = acc.Login;
                command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = acc.Password;
                //Check if user exists or not.
                try {
                    OpenConnection();
                    MySqlDataReader dr;
                    dr = command.ExecuteReader();
                    while (dr.Read()) {
                        output = dr.GetString(column);
                    }
                    dr.Close();
                    CloseConnection();
                    return output;
                }
                catch (Exception ex) {
                    //TODO: Implement custom dialog system.
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Changes given cmd's values.
        /// </summary>
        /// <param name="cmd">MySQL command.</param>
        /// <param name="val">What to change.</param>
        /// <param name="vnc">With what to change.</param>
        /// <returns></returns>
        static MySqlCommand ChangeCmd(MySqlCommand cmd, string val, object vnc) { if (!App.RunMySQLCommands) return null; cmd.CommandText = string.Format(cmd.CommandText, val, vnc, RegistrySLS.LoadString("LastID")); return cmd; }

        /// <summary>
        /// Changes data in given column. (Deferences user by ID.)
        /// </summary>
        /// <param name="dataType">Special indeficator to difirenciate column and database data type (Varchar, Int, etc.).</param>
        /// <param name="value">Data to replace old data.</param>
        public static void ChangeData(DBDataType dataType, object value) {
            if (!App.RunMySQLCommands) return;
            MySqlCommand command = new MySqlCommand("UPDATE `xgl_publishers` SET `{0}`='{1}' WHERE `id`={2}", Connection);
            if (string.IsNullOrEmpty(value.ToString()))
                command = new MySqlCommand("UPDATE `xgl_publishers` SET `{0}`='INS' WHERE `id`={2}", Connection);
            switch (dataType) {
                case DBDataType.DT_NAME:
                    command = ChangeCmd(command, "name", value);
                    break;
                case DBDataType.DT_ICON:
                    command = ChangeCmd(command, "icon", value);
                    break;
                case DBDataType.DT_EMAIL:
                    command = ChangeCmd(command, "email", value);
                    break;
                case DBDataType.DT_PASSWORD:
                    command = ChangeCmd(command, "password", value);
                    break;
                case DBDataType.DT_ACTIVITY:
                    command = new MySqlCommand("UPDATE `xgl_publishers` SET `{0}`={1} WHERE `id`={2}", Connection);
                    command = ChangeCmd(command, "activity", value);
                    break;
                case DBDataType.DT_DESCRIPTION:
                    command = ChangeCmd(command, "description", value);
                    break;
                case DBDataType.DT_PRODUCTS:
                    command = ChangeCmd(command, "productIDs", value);
                    break;
                case DBDataType.DT_PERSONALIZATIONVALUES:
                    command = ChangeCmd(command, "personalizationChoices", value);
                    break;
                case DBDataType.DT_COMMANDIDS:
                    command = ChangeCmd(command, "commandIDs", value);
                    break;
            }
            SendData(command);
        }

        static void SendData(MySqlCommand command) {
            if (!App.RunMySQLCommands) return;
            //Start the connection.
            OpenConnection();
            //Execute the data query.
            if (command.ExecuteNonQuery() == 1) {
                //Close the connection.
                CloseConnection();
            }
        }

    }

}
