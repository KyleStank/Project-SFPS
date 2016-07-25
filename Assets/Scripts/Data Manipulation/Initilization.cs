using UnityEngine;

[DisallowMultipleComponent]
public sealed class Initilization : MonoBehaviour {
    private void LoadGameSettings() { //Loads all of the game's settings
        //Game settings
        GameManager.Instance.settings = Serialization.LoadFromXML(GameManager.Instance.settings, Paths.localPath, Paths.Files.gameSettings);
    }

    private void Awake() {
        LoadGameSettings(); //Loads game settings
    }
}
