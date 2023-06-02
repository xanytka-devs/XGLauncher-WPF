using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGL.SLS {

    public class RegistrySLS {

        static readonly RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true);
        static RegistryKey curKey;

        public static void Save(string name, object value) {
            if (RegHasXGLSubKey()) {
                curKey = softwareKey.OpenSubKey("Xanytka Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, value);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                curKey = softwareKey.OpenSubKey("Xanytka Software");
                curKey.SetValue(name, value);
            }
        }

        public static bool LoadBool(string name) { return bool.Parse(Load(name, false).ToString()); }
        public static bool LoadBool(string name, bool def) { return bool.Parse(Load(name, def).ToString()); }
        public static string LoadString(string name) { return Load(name, false).ToString(); }
        public static string LoadString(string name, string def) { return Load(name, def).ToString(); }
        public static int LoadInt(string name) { return int.Parse(Load(name, false).ToString()); }
        public static int LoadInt(string name, int def) { return int.Parse(Load(name, def).ToString()); }

        public static object Load(string name) {
            if (RegHasXGLSubKey())  {
                curKey = softwareKey.OpenSubKey("Xanytka Software");
                return curKey.GetValue(name);
            } else {
                softwareKey.CreateSubKey("Xanytka Software");
                softwareKey.CreateSubKey(name);
                return null;
            }
        }

        public static object Load(string name, object def) {
            if (RegHasXGLSubKey()) {
                curKey = softwareKey.OpenSubKey("Xanytka Software");
                return curKey.GetValue(name, def);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey = softwareKey.OpenSubKey("Xanytka Software", true);
                curKey.SetValue(name, def);
                return def;
            }
        }

        static bool RegHasXGLSubKey() { return softwareKey.GetSubKeyNames().Contains("Xanytka Software"); }

        public static void Reset() {
            softwareKey.DeleteSubKey("Xanytka Software");
            Setup();
        }

        public static void Setup() {
            softwareKey.CreateSubKey("Xanytka Software");
            curKey = softwareKey.OpenSubKey("Xanytka Software");
            Save("Theme", "Dark");
        }

    }

    public class JSonSLS {
        public static void Write(string path, string fileName, string[] text) {
            string serializedText = JsonConvert.SerializeObject(text);
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, fileName), serializedText);
        }
        public static string Read(string path, string fileName) {
            string serializedText = File.ReadAllText(Path.Combine(path, fileName));
            object deserializedText = JsonConvert.DeserializeObject(serializedText);
            return deserializedText.ToString();
        }
    }

}