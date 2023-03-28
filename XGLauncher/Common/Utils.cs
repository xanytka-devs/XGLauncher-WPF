using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace XGL {

    public class XNetCode {

        public static void Encode(string s) {

        }

    }

    public class Utils {

        public static bool NotEmptyAndNotNotSet(string val) {
            return !string.IsNullOrEmpty(val) &&
                                val != "INS";
        }

        public static string ReaplaceValueWith(string source, int index, object value) {
            string[] tmp = source.Split(';');
            string output = string.Empty;

            for (int i = 0; i < tmp.Length; i++){
                if(i == index) tmp[i] = value.ToString();
                if (!string.IsNullOrEmpty(tmp[i])) output += tmp[i] + ";";
            }

            return output;
        }

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

        public static int BoolToInt(bool input) { return input ? 1 : 0; }
        public static object BoolToObj(bool input, object tV, object fV) { return input ? tV : fV; }
        public static bool BoolFromInt(int input) { if (input == 0) return false; else return true; }
        public static bool BoolFromObj(object tV, object fV) { if (tV == fV) return true; else return false; }
        public static bool? BoolFromString(string v) {
            switch (v) {
                case "0": return false;
                case "1": return true;
                default: return null;
            }
        }

        public static long GetTotalFreeSpace(string driveName) {
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (drive.IsReady && drive.Name == driveName) {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }

        public static async Task DownloadFileAsync(string url, string fileName) {
            bool isReady = false;
            try {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => isReady = true;
                await Task.Run(() => client.DownloadFileAsync(new Uri(url), fileName));
                for (int i = 0; i < int.MaxValue; i++) {
                    if (isReady) break;
                }
                return;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "XGLauncher");
                Debug.WriteLine(ex);
                return;
            }

        }

        public static long GetDirectorySize(DirectoryInfo d) {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) size += fi.Length;
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) size += GetDirectorySize(di);
            return size;
        }

        public long GetFileSizeFromURL(string url) {
            long result = -1;
            WebRequest req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (WebResponse resp = req.GetResponse()) {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                    result = ContentLength;
            }
            return result;
        }

        public static bool IsFileLocked(FileInfo file) {
            try {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                    stream.Close();
                }
            }
            catch (IOException) {
                return true;
            }
            return false;
        }

        public static bool IsControlVisible(UIElement c) {
            if (!c.IsVisible)
                return false;
            else if (c is FrameworkElement fe && (fe.ActualWidth == 0 || fe.ActualHeight == 0))
                return false;
            else {
                UIElement parent = VisualTreeHelper.GetParent(c) as UIElement;
                return parent == null ? c.IsVisible : IsControlVisible(parent);
            }
        }

    }

}
