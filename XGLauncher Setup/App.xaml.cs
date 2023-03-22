using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace XGLS {

    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    
    public partial class App : Application {

        public static string DBConnectorData { get; private set; }
        public static string Version { get; private set; } = "0.1";

        protected override void OnStartup(StartupEventArgs e) {
            string[] appSData = INTERNAL.ApplicationSData.IndefData;
            DBConnectorData = appSData[0];
            base.OnStartup(e);
        }

    }
}
