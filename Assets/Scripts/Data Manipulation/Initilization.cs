using UnityEngine;

public sealed class Initilization : MonoBehaviour {
    private void CreateDataDirectories() { //Create all of the needed directories
        Serialization.CreateDirectory(Paths.dataDirectory);
    }

    private void LoadGameSettings() { //Loads all of the game's settings
        //Game settings
        GameManager.Instance.settings = Serialization.LoadFromXML(GameManager.Instance.settings, Paths.dataDirectory, Paths.Files.gameSettings);

        //Player settings
        PlayerManager.Instance.settings = Serialization.LoadFromXML(PlayerManager.Instance.settings, Paths.dataDirectory, Paths.Files.playerSettings);
    }

    private void Awake() {
        CreateDataDirectories(); //Creates the "Data" directory
        LoadGameSettings(); //Loads game settings
    }
}
