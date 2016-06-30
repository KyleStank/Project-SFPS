using UnityEngine;
using System.Collections;

public static class Paths {
    //Main directories
    public static readonly string localPath = Application.dataPath + "/";
    public static readonly string dataDirectory = localPath + "Data/";

    //Settings' directoryes
    public static readonly string settingsDirectory = "Settings/";

    public sealed class Files {
        //Settings' files
        public static readonly string gameSettings = "game_settings.xml";
        public static readonly string playerSettings = "player_settings.xml";
        public static readonly string enemySettings = "enemy_settings.xml";
    }
}
