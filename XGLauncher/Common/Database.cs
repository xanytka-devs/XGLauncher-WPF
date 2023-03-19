using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using XGL.SLS;

namespace XGL.Networking.Database {

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
            DT_USERNAME,
            DT_ICON,
            DT_EMAIL,
            DT_PASSWORD,
            DT_LASTONLINE,
            DT_ACTIVITY,
            DT_DESCRIPTION,
            DT_FIRSTNAME,
            DT_LASTNAME,
            DT_PUBLICRIGHTS,
            DT_PRODUCTSSAVED,
            DT_PERSONALIZATIONVALUES,
        }

        static bool AccountCreated = false;
        public static string[] AppsOnAccount;
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
            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @log and `password` = @pass", Connection);
            //Add parameters
            command.Parameters.Add("@log", MySqlDbType.VarChar).Value = account.Login;
            command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = account.Password;
            //Initialize adapter
            adapter.SelectCommand = command;
            adapter.Fill(table);
            //Check if user exists or not.
            if (table.Rows.Count > 0) {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) {
                    log = dr.GetString("login");
                }
                dr.Close();
                if (log == account.Login) return true;
                CloseConnection();
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
        /// Saves all data about products.
        /// </summary>
        /// <returns>true=success;false=error</returns>
        static bool AccountAppsCheck() {
            if(!App.RunMySQLCommands) return false;
            try {
                if (App.CurrentAccount.Login != "INS" && App.CurrentAccount.Login.ToLower() != "not set") {
                    AppsOnAccount = GetValue(App.CurrentAccount, "productsSaved")
                            .ToString().Split(';');
                    if (AppsOnAccount.Length > 1)
                        AppsOnAccount = AppsOnAccount.Where((val, idx) => idx != AppsOnAccount.Length - 1).ToArray();
                    return true;
                } else {
                    AppsOnAccount = new string[]{ "-" };
                    return true;
                }
            }
            catch {
                return false;
            }
        }
        /// <summary>
        /// Tries to parse products.
        /// </summary>
        public static void ParseApps() {
            try {
                for (int i = 0; i < 4; i++) {
                    if (AccountAppsCheck()) break;
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "XGLauncher");
                throw;
            }
        }
        /// <summary>
        /// Gets all ids of products on account in string like "1,2'.
        /// </summary>
        /// <returns>String like "1,2"</returns>
        public static string GetAppIDs() {
            string Out = string.Empty;
            if (!App.RunMySQLCommands) return Out;
            if (!string.IsNullOrEmpty(AppsOnAccount[0])) {
                if (AppsOnAccount[0] != "*") {
                    for (int i = 0; i < AppsOnAccount.Length; i++) {
                        if (i != AppsOnAccount.Length - 1) Out += AppsOnAccount[i] + ", ";
                        else Out += AppsOnAccount[i];
                    }
                } else Out = AppsOnAccount[0];
            }
            return Out;
        }
        /// <summary>
        /// Checks if such row exists in Database.
        /// </summary>
        /// <param name="column">Column with instance of row.</param>
        /// <param name="value">Value, defined in column.</param>
        /// <param name="valueType">Type of passed data.</param>
        /// <returns>true=exists;false=doesn't exist</returns>
        public static bool RowExisting(string column, string value, MySqlDbType valueType) {
            if (!App.RunMySQLCommands) return false;
            //Initialize components
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand($"SELECT * FROM `users` WHERE `{column}`=@val", Connection);
            //Add parameters
            command.Parameters.Add("@val", valueType).Value = value;
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
        /// Gets passed user's ID.
        /// </summary>
        /// <param name="account">User to search for.</param>
        /// <returns>string(string(#0000);int(0))</returns>
        public static string GetID(Account account) {
            if (!App.RunMySQLCommands) return null;
            if (AccountExisting(account)) {
                //Initialize components
                long output = 0;
                MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @log and `password` = @pass", Connection);
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
                    string outputed = output.ToString();
                    if (outputed.Length < 2)
                        outputed = "000" + outputed;
                    else if (outputed.Length < 3)
                        outputed = "00" + outputed;
                    else if (outputed.Length < 4)
                        outputed = "0" + outputed;
                    else if (outputed.Length < 5)
                        outputed.Remove(outputed.Length - 1, outputed.Length - 4);
                    return $"#{outputed};{output}";
                }
                catch (Exception ex) {
                    //TODO: Implement custom dialog system.
                    MessageBox.Show(ex.Message);
                    return "#null;0";
                }
            }
            return "#null;0";
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
                MySqlCommand command = new MySqlCommand($"SELECT `{column.ToLower()}` FROM `users` WHERE `login` = @log and `password` = @pass", Connection);
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
        /// Creates new account on Database.
        /// </summary>
        /// <param name="account">Private account info.</param>
        /// <param name="email">Email for account.</param>
        /// <returns>true=created successfuly;false=creation failed</returns>
        public static bool CreateAccount(Account account, string username, string email) {
            if (!App.RunMySQLCommands) return false;
            if (!AccountCreated) {
                App.RunMySQLCommands = false;
                //Anti-spam. (Sometimes this function creates 3 rows,
                //  so this boolean value stops two invalid requests from happening.)
                AccountCreated = true;
                //Define Command.
                MySqlCommand command = new MySqlCommand("INSERT INTO `users`(`login`, `username`, `email`, `password`, `joinedOn`, `activity`, `productsSaved`) VALUES(@log,@usn,@email,@pass,@jon,@act,@psaved)", Connection);
                //Define parameters.
                string password = account.Password;
                command.Parameters.Add("@log", MySqlDbType.VarChar).Value = account.Login;
                command.Parameters.Add("@usn", MySqlDbType.VarChar).Value = username;
                command.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;
                command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = password;
                command.Parameters.Add("@jon", MySqlDbType.Date).Value = DateTime.Now.ToString("yyyy-MM-dd");
                command.Parameters.Add("@act", MySqlDbType.Int16).Value = 1;
                command.Parameters.Add("@psaved", MySqlDbType.LongText).Value = "-";
                RegistrySLS.Save("LoginData", account.Login + ";" + password);
                App.RunMySQLCommands = true;
                //Start the connection.
                OpenConnection();
                //Execute the data query.
                if (command.ExecuteNonQuery() == 1) {
                    //Close the connection.
                    CloseConnection();
                    AccountCreated = false;
                    return true;
                } else {
                    //Close the connection.
                    CloseConnection();
                    AccountCreated = false;
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// Changes given cmd's values.
        /// </summary>
        /// <param name="cmd">MySQL command.</param>
        /// <param name="val">What to change.</param>
        /// <param name="vnc">With what to change.</param>
        /// <returns></returns>
        static MySqlCommand ChangeCmd(MySqlCommand cmd, string val, object vnc) { if (!App.RunMySQLCommands) return null; cmd.CommandText = string.Format(cmd.CommandText, val, vnc, RegistrySLS.LoadString("LastID").Split(';')[1]); return cmd; }
        /// <summary>
        /// Changes data in given column. (Deferences user by ID.)
        /// </summary>
        /// <param name="dataType">Special indeficator to difirenciate column and database data type (Varchar, Int, etc.).</param>
        /// <param name="value">Data to replace old data.</param>
        public static void SetValue(DBDataType dataType, object value) {
            if (!App.RunMySQLCommands) return;
            MySqlCommand command = new MySqlCommand("UPDATE `users` SET `{0}`='{1}' WHERE `id`={2}", Connection);
            if (string.IsNullOrEmpty(value.ToString()))
                command = new MySqlCommand("UPDATE `users` SET `{0}`='INS' WHERE `id`={2}", Connection);
            switch (dataType) {
                case DBDataType.DT_USERNAME:
                    command = ChangeCmd(command, "username", value);
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
                case DBDataType.DT_LASTONLINE:
                    command = new MySqlCommand("UPDATE `users` SET `{0}`=current_timestamp() WHERE `id`={2}", Connection);
                    command = ChangeCmd(command, "lastOnline", value);
                    break;
                case DBDataType.DT_ACTIVITY:
                    command = ChangeCmd(command, "activity", value);
                    break;
                case DBDataType.DT_DESCRIPTION:
                    command = ChangeCmd(command, "description", value);
                    break;
                case DBDataType.DT_FIRSTNAME:
                    command = ChangeCmd(command, "firstname", value);
                    break;
                case DBDataType.DT_LASTNAME:
                    command = ChangeCmd(command, "secondname", value);
                    break;
                case DBDataType.DT_PUBLICRIGHTS:
                    command = ChangeCmd(command, "publicRights", value);
                    break;
                case DBDataType.DT_PRODUCTSSAVED:
                    command = ChangeCmd(command, "productsSaved", value);
                    break;
                case DBDataType.DT_PERSONALIZATIONVALUES:
                    command = ChangeCmd(command, "personalizationChoices", value);
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
