using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XGL {

    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    
    public partial class App : Application {

        public static long UpdateID { get; private set; }
        public static string DBConnectorData { get; private set; }
        public static int UpdateIteration { get; private set; } = 0;

        protected override void OnStartup(StartupEventArgs e) {
            foreach (string arg in e.Args) {
                if(arg == "/old") UpdateIteration = 1;
                else if(arg == "/new") UpdateIteration = 2;
            }
            string[] appSData = INTERNAL.ApplicationSData.IndefData;
            DBConnectorData = appSData[0];
            base.OnStartup(e);
        }

    }
}
