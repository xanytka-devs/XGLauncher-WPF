using System.IO;
using System.Linq;

namespace XGLS {
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

}
