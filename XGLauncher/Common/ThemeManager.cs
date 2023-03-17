using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace XGL {

    public class ThemeManager {

        readonly static List<Brush> br = new List<Brush>();

        public static Brush FontBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static Brush DisabledFontBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        public static Brush SelectBrush = new SolidColorBrush(Color.FromRgb(37, 37, 37));
        public static Brush SemiLightSelectBrush = new SolidColorBrush(Color.FromRgb(38, 38, 39));
        public static Brush LightSelectBrush = new SolidColorBrush(Color.FromRgb(57, 57, 57));
        public static Brush LighterSelectBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68));

        public static Brush WindowBrush = new SolidColorBrush(Color.FromRgb(48, 48, 49));
        public static Brush DialogWindowBrush = new SolidColorBrush(Color.FromRgb(50, 50, 51));

        public static Brush ThematicBrush = new SolidColorBrush(Color.FromRgb(244, 155, 54));
        public static Brush SelectThematicBrush = new SolidColorBrush(Color.FromRgb(245, 165, 74));
        public static Brush DarkThematicBrush = new SolidColorBrush(Color.FromRgb(196, 132, 59));

        public static void Instatiate(string themeName) {

            br.Add(FontBrush);
            br.Add(SelectBrush);
            br.Add(LightSelectBrush);
            br.Add(LighterSelectBrush);
            br.Add(WindowBrush);
            br.Add(DialogWindowBrush);
            br.Add(ThematicBrush);
            br.Add(SelectThematicBrush);
            br.Add(DarkThematicBrush);

            switch (themeName) {
                /*case "dark":
                    UpdateThemeValues("255|255|255;");
                    break;*/
                default:
                    break;
            }

        }

        internal static void UpdateThemeValues(string themePlast) {

            string[] vs = themePlast.Split(';');
            for (int i = 0; i < 10; i++) {
                string[] v = vs[i].Split('|');
                br[i] = FromRGB(byte.Parse(v[0]), byte.Parse(v[1]), byte.Parse(v[2]));
            }

        }

        static SolidColorBrush FromRGB(byte r, byte g, byte b) { return new SolidColorBrush(Color.FromRgb(r, g, b)); }

    }

}
