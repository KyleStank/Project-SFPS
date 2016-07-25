using UnityEngine;
using System.Collections;

public static class Paths {
    //Main directories
    public static readonly string localPath = Application.dataPath + "/";

    public sealed class Files {
        public static readonly string gameSettings = "game_settings.xml";
    }
}
