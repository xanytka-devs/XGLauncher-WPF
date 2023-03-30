using System;
using System.IO;

namespace XGL.Common {

    public class Game {

        public string Name;
        public string Directory;
        public string Executable;
        public GameData Data;

        public Game(string name) {
            Data = new GameData(0, name);
            //Instantiate params.
            string exe = $"{name}.exe";
            Name = name;
            Directory = Path.Combine(Environment.CurrentDirectory, $@"apps\{name}");
            Executable = Path.Combine(Directory, exe);
        }

    }

    public class GameData {

        public long ID;
        public string AppName;
        public string AppVersion;
        public bool Updated;
        public bool Avaibility;
        public DateTime LastTimePlayed;
        public float TotalTime;

        public GameData(long id, string appName) {
            ID = id;
            AppName = appName;
        }

    }

    public class GameManager {

    }

}