using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace XGL {

    public class Utils {

        public static Utils I;

        public Utils() { I = this; }

        public bool NotEmptyAndNotINS(string val) {
            return !string.IsNullOrEmpty(val) &&
                                val != "INS";
        }

        public string ReaplaceValueWith(string source, int index, object value) {
            string[] tmp = source.Split(';');
            string output = string.Empty;

            for (int i = 0; i < tmp.Length; i++){
                if(i == index) tmp[i] = value.ToString();
                if (!string.IsNullOrEmpty(tmp[i])) output += tmp[i] + ";";
            }

            return output;
        }

        public string StringWithoutValueBySplit(string original, char charToSplit, int indexToRemove, bool addCharToSplit = false) {
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

        public int BoolToInt(bool input) { return input ? 1 : 0; }
        public object BoolToObj(bool input, object tV, object fV) { return input ? tV : fV; }
        public bool BoolFromInt(int input) { if (input == 0) return false; else return true; }
        public bool BoolFromObj(object tV, object fV) { if (tV == fV) return true; else return false; }
        public bool? BoolFromString(string v) {
            switch (v) {
                case "0": return false;
                case "1": return true;
                default: return null;
            }
        }

        public long GetTotalFreeSpace(string driveName) {
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (drive.IsReady && drive.Name == driveName) {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }

        public void AddToStartMenu(string name, string path) {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), name))) return;
            else File.Copy(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), name));
        }

        public async Task DownloadFileAsync(string url, string fileName) {
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

        public string GetDirectorySize(DirectoryInfo d) {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) size += fi.Length;
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) size += long.Parse(GetDirectorySize(di));
            return SizeSuffix(size);
        }

        readonly string[] SizeSuffixes =
                   { "Bit", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public string SizeSuffix(long value, int decimalPlaces = 1) {
            SizeSuffixes[0] = LocalizationManager.I.dictionary["sw.g.bit"];
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            int mag = (int)Math.Log(value, 1024);

            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000) {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
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

        public bool IsFileLocked(FileInfo file) {
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

        public bool IsControlVisible(FrameworkElement element, FrameworkElement container) {
            if (!element.IsVisible) return false;
            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);

        public string GetSystemTheme() {
            StringBuilder themeNameBuffer = new StringBuilder(260);
            var error = GetCurrentThemeName(themeNameBuffer, themeNameBuffer.Capacity, null, 0, null, 0);
            if (error != 0) Marshal.ThrowExceptionForHR(error);
            return themeNameBuffer.ToString();
        }

        public HorizontalAlignment CharToHA(char v) {
            switch (v) {
                case 'c':
                    return HorizontalAlignment.Center;
                case 'l':
                    return HorizontalAlignment.Left;
                case 'r':
                    return HorizontalAlignment.Right;
                case 's':
                    return HorizontalAlignment.Stretch;
                default:
                    return HorizontalAlignment.Center;
            }
        }

        public VerticalAlignment CharToVA(char v) {
            switch (v) {
                case 'c':
                    return VerticalAlignment.Center;
                case 't':
                    return VerticalAlignment.Top;
                case 'b':
                    return VerticalAlignment.Bottom;
                case 's':
                    return VerticalAlignment.Stretch;
                default:
                    return VerticalAlignment.Center;
            }
        }
    }

}