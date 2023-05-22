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
                                val != "Not Set";
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

        public static bool DownloadImage(string url, string fileName) {
            try {
                /*using (WebClient client = new WebClient()) {
                    client.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {

                    };
                    client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs e) => {

                    };
                    client.DownloadFileAsync(new Uri(link), Path.Combine(path, "XGLauncher.zip"));
                }*/
                Uri urlUri = new Uri(url);
                var request = WebRequest.CreateDefault(urlUri);
                byte[] buffer = new byte[4096];
                using (var target = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                    using (var response = request.GetResponse()) {
                        using (var stream = response.GetResponseStream()) {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
                                target.Write(buffer, 0, read);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "XGLauncher");
                Debug.WriteLine(ex);
                return false;
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
            if (c.IsVisible == false)
                return false;
            else
                if (VisualTreeHelper.GetParent(c) as UIElement != null)
                return IsControlVisible(VisualTreeHelper.GetParent(c) as UIElement);
            else
                return c.IsVisible;
        }

    }

}
