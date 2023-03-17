using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;

namespace XGL {
    public class Utils {
        public static string StringWithoutValueBySplit(string original, char charToSplit, int indexToRemove, bool addCharToSplit = false) {
            string[] parts = original.Split(charToSplit);
            string output = string.Empty;
            for (int i = 0; i < parts.Length; i++) {
                if (i != indexToRemove) {
                    output += parts[i];
                    if (addCharToSplit) output += charToSplit.ToString();
                }
            }
            return output;
        }
        public static bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        public static bool IsRootDirectory(string path) {
            DirectoryInfo d = new DirectoryInfo(path);
            if (d.Parent == null) return true;
            return false;
        }
    }
    public class Database {
        internal static readonly MySqlConnection Connection = new MySqlConnection("server-connector-data");
        internal static bool OpenConnection() {
            try {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        internal static bool CloseConnection() {
            try {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        public static string GetValue() {
            string output = string.Empty;
            MySqlCommand command = new MySqlCommand($"SELECT `latest` FROM `applications` WHERE `id` = {MainWindow.Instance.installID.ToString()}", Connection);
            try {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) output = dr.GetString("latest");
                dr.Close();
                CloseConnection();
                return output;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public static string GetValueUpdate() {
            string output = string.Empty;
            MySqlCommand command = new MySqlCommand($"SELECT `latest` FROM `applications` WHERE `id` = 2", Connection);
            try {
                OpenConnection();
                MySqlDataReader dr;
                dr = command.ExecuteReader();
                while (dr.Read()) output = dr.GetString("latest");
                dr.Close();
                CloseConnection();
                return output;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
