using Microsoft.Win32;
using System.Diagnostics;
internal class Program {
    private static void Main(string[] args) {
        string[] termPaths = { Path.Combine(Environment.CurrentDirectory, "localizations"),
            Path.Combine(Environment.CurrentDirectory, "cache"), Path.Combine(Environment.CurrentDirectory, "app.publish"),
            Path.Combine(Environment.CurrentDirectory, "apps"), Path.Combine(Environment.CurrentDirectory, "logs"),
            Path.Combine(Environment.CurrentDirectory, "Uninstall XGLauncher.exe"), 
            Path.Combine(Environment.CurrentDirectory, "XGLauncher Updater.exe")};
        //Define variables.
        string fileName = "uninstall.bat";
        string filePath = Environment.CurrentDirectory;
        string fileText = $"del /F /Q \"{StringWithoutValueBySplit(filePath, '\\', filePath.Length - 1, true)}\"";
        string fullPath = Path.Combine(StringWithoutValueBySplit(filePath, '\\', filePath.Length - 1, true), fileName);
        //Delete all folders in root (application folder).
        for (int i = 0; i < termPaths.Length; i++) {
            if(Directory.Exists(termPaths[i])) {
                DirectoryInfo di = new DirectoryInfo(termPaths[i]);
                foreach (FileInfo file in di.GetFiles()) file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                Directory.Delete(termPaths[i]);
            }
        }
        string sh = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "XGLauncher.lnk";
        if(File.Exists(sh)) File.Delete(sh);
        if(Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true).GetValueNames().Contains("XGLauncher"))
            Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true).DeleteSubKey("XGLauncher");
        //Write and run .bat to delete left over files (.exe, .dll, .lib, etc.)
        File.WriteAllText(fullPath, fileText);
        Process.Start(fullPath);
        static string StringWithoutValueBySplit(string original, char charToSplit, int indexToRemove, bool addCharToSplit = false) {
            string[] parts = original.Split(charToSplit);
            string output = string.Empty;
            for (int i = 0; i < parts.Length; i++) {
                if(i != indexToRemove) {
                    output += parts[i];
                    if(addCharToSplit) output += charToSplit.ToString();
                }
            }
            return output;
        }
    }
}