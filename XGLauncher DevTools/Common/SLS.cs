using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGL.SLS {

    public class RegistrySLS {

        static RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true);
        static RegistryKey launcherKey = softwareKey.OpenSubKey("Xanytka Software", true);
        static RegistryKey curKey;

        public static void Save(string name, object value) {
            if(RegHasXGLSubKey()) {
                curKey = launcherKey.OpenSubKey("DevTools", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, value);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                launcherKey.CreateSubKey("DevTools", true);
                curKey = launcherKey.OpenSubKey("DevTools", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, value);
            }
        }

        public static bool LoadBool(string name) { return bool.Parse(Load(name, false).ToString()); }
        public static bool LoadBool(string name, bool def) { return bool.Parse(Load(name, def).ToString()); }
        public static string LoadString(string name) { return Load(name, false).ToString(); }
        public static string LoadString(string name, string def) { return Load(name, def).ToString(); }
        public static string LoadXGLString(string name) { return LoadXGL(name, false).ToString(); }
        public static string LoadXGLString(string name, string def) { return LoadXGL(name, def).ToString(); }
        public static int LoadInt(string name) { return int.Parse(Load(name, false).ToString()); }
        public static int LoadInt(string name, int def) { return int.Parse(Load(name, def).ToString()); }

        public static object Load(string name) {
            if(RegHasXGLSubKey())  {
                curKey = launcherKey.OpenSubKey("DevTools");
                return curKey.GetValue(name);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                launcherKey.CreateSubKey("DevTools", true);
                curKey = launcherKey.OpenSubKey("DevTools", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, string.Empty);
                return null;
            }
        }

        public static object Load(string name, object def) {
            if(RegHasXGLSubKey()) {
                curKey = launcherKey.OpenSubKey("DevTools");
                return curKey.GetValue(name, def);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                launcherKey.CreateSubKey("DevTools", true);
                curKey = launcherKey.OpenSubKey("DevTools", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, def);
                return def;
            }
        }

        public static object LoadXGL(string name) {
            if(RegHasXGLSubKey()) {
                curKey = launcherKey;
                return curKey.GetValue(name);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                launcherKey.CreateSubKey("DevTools", true);
                curKey = softwareKey.OpenSubKey("Xanytka Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, string.Empty);
                return null;
            }
        }

        public static object LoadXGL(string name, object def) {
            if(RegHasXGLSubKey()) {
                curKey = launcherKey;
                return curKey.GetValue(name, def);
            } else {
                softwareKey.CreateSubKey("Xanytka Software", true);
                launcherKey.CreateSubKey("DevTools", true);
                curKey = softwareKey.OpenSubKey("Xanytka Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                curKey.SetValue(name, def);
                return def;
            }
        }

        static bool RegHasXGLSubKey() { return softwareKey.GetSubKeyNames().Contains("Xanytka Software") && launcherKey.GetSubKeyNames().Contains("DevTools"); }

        public static void Reset() {
            softwareKey.DeleteSubKey("Xanytka Software");
            Setup();
        }

        public static void Setup() {
            softwareKey.CreateSubKey("Xanytka Software", true);
            launcherKey.CreateSubKey("DevTools", true);
            curKey = launcherKey.OpenSubKey("DevTools", RegistryKeyPermissionCheck.ReadWriteSubTree);
        }

    }

}
