using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour {
    //Singelton
    private static GameManager _instance;

    public static GameManager Instance {
        get {
            if(_instance == null) { //If this game object doesn't exist
                GameObject gm = new GameObject("Game Manager"); //Create game manager game object
                gm.AddComponent<GameManager>(); //Add class to newly created game object
            }

            return _instance;
        }
    }

    /* Game Settings */
    public GameSettings settings = new GameSettings();

    /* non-XML Settings */
    public MatchSettings matchSettings = new MatchSettings();

    public Dictionary<string, AudioClip> networkAudioClips = new Dictionary<string, AudioClip>();

    private void Awake() {
        _instance = this; //Singelton creation
    }

    /* Methods */
    //Scenes
    public void ReloadCurrentScene() { //Reloads the currently active scene
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    //Cameras
    public void ToggleSceneCamera(bool active) { //Toggles the scene's camera
        string sceneCamName = "Scene Camera";
        GameObject gm = GameObject.Find(sceneCamName);

        if(gm) {
            gm.GetComponent<Camera>().enabled = active;
            gm.GetComponent<AudioListener>().enabled = active;
        }
    }

    //Audio
    public void AddAudio(string clipID, AudioClip clip) { //Adds an AudioClip to the list of audio used on the network
        if(clip) //If an AudioClip was given
            if(!GetAudioClip(clipID)) //If audio clip doesn't already exist
                networkAudioClips.Add(clipID, clip); //Add audio clip
    }

    public AudioClip GetAudioClip(string clipID) { //Returns an AudioClip inside the audio list
        if(networkAudioClips.ContainsKey(clipID))
            return networkAudioClips[clipID];
        else
            return null;
    }

    /* Non-XML classes */
    public sealed class MatchSettings {
        public float respawnDelay = 3.0f;
    }
}

[XmlRoot("game_settings")]
public sealed class GameSettings {
    //Lets us access the game's settings
    [XmlIgnore]
    public CursorManagement cursorManagement = new CursorManagement();
    [XmlIgnore]
    public Tags tags = new Tags();
    [XmlElement("key_mappings")]
    public Keys keys = new Keys();

    /* Cursor Management */
    public sealed class CursorManagement {
        public void SetState(CursorLockMode state) { //Set state of cursor
            Cursor.lockState = state;

            //Check if cursor should or shouldn't be visible
            if(GetState() == CursorLockMode.None)
                SetVisible(true);
            else
                SetVisible(false);
        }

        public CursorLockMode GetState() { //Returns state of cursor
            return Cursor.lockState;
        }

        public void SetVisible(bool visible) { //Shows or hide the cursor
            Cursor.visible = visible;
        }
    }

    /* Tags */
    public sealed class Tags {
        //List of every tag
        public string obtainable = "Obtainable";
        public string outOfBounds = "Out of Bounds";
        public string levelRequirement = "Level Requirement";
        public string booster = "Booster";
        public string playerBoundary = "Player Boundary";
        public string enemy = "Enemy";
        public string bullet = "Bullet";
        public string safeRigidbody = "Safe Rigidbody";
        public string destructible = "Destructible";
    }

    /* Key Mapping */
    public sealed class Keys {
        //List of all keys used in game
        [XmlElement("activate_key")]
        public KeyCode activateKey = KeyCode.E;
        [XmlElement("x_axis_key")]
        public KeyCode xAxisKey = KeyCode.X;
        [XmlElement("y_axis_key")]
        public KeyCode yAxisKey = KeyCode.Y;
        [XmlElement("z_axis_key")]
        public KeyCode zAxisKey = KeyCode.Z;
        [XmlElement("primary_mouse_button")]
        public int primaryMouseClick = 0;
        [XmlElement("secondary_mouse_button")]
        public int secondaryMouseClick = 1;
    }
}
