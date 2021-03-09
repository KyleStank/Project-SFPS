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

    /* Non-XML Settings */
    public MatchSettings matchSettings = new MatchSettings();
    public CursorManagement cursor = new CursorManagement();

    //Other variables
    public Dictionary<string, AudioClip> networkAudioClips = new Dictionary<string, AudioClip>();
    public bool isGamePaused;

    private void Awake() {
        _instance = this; //Singelton creation

        //cursor.SetState(CursorLockMode.Confined); TODO: Look at this.
    }

    /* Methods */
    //Pausing
    public void PauseGame(bool pause) { //Pauses or unpauses the game
        isGamePaused = pause;
    }
    
    //Scenes
    public void ReloadCurrentScene() { //Reloads the currently active scene
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    //Cameras
    public void ToggleSceneCamera(bool active) { //Toggles the scene's camera
        //Get the correct camera
        string sceneCamName = "Scene Camera";
        GameObject gm = GameObject.Find(sceneCamName);

        //Components that should be disabled
        if(gm) {
            gm.GetComponent<Camera>().enabled = active;
            gm.GetComponent<AudioListener>().enabled = active;
        }

        //Enable or disable any UI
        UIManager.Instance.player.ammoUI.gameObject.SetActive(!active);

        //Shows or hides the cursor
        cursor.SetVisible(active);
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

    /* Cursor Management */
    public sealed class CursorManagement {
        public void SetState(CursorLockMode state) { //Set state of cursor
            Cursor.lockState = state;
        }

        public CursorLockMode GetState() { //Returns state of cursor
            return Cursor.lockState;
        }

        public void SetVisible(bool visible) { //Shows or hide the cursor
            Cursor.visible = visible;
        }
    }
}

[XmlRoot("game_settings")]
public sealed class GameSettings {
    //Lets us access the game's settings
    [XmlElement("is_initial_startup")]
    public bool isInitialStartup = true;
    [XmlElement("player_username")]
    public string username = "AnonymousPlayer";

    [XmlElement("key_mappings")]
    public Keys keys = new Keys();

    [XmlIgnore]
    public MovementSettings moveSettings = new MovementSettings();

    [XmlElement("camera_settings")]
    public CameraSettings camSettings = new CameraSettings();

    /* Key Mapping */
    public sealed class Keys {
        //List of all keys used in game
        [XmlElement("activate_key")]
        public KeyCode activateKey = KeyCode.E;

        [XmlElement("primary_mouse_button")]
        public int primaryMouseClick = 0;

        [XmlElement("secondary_mouse_button")]
        public int secondaryMouseClick = 1;

        [XmlElement("sprint_key")]
        public KeyCode sprintKey = KeyCode.LeftShift;
    }

    /* Movement Settings */
    public sealed class MovementSettings { //Houses base settings for the player's movement
        //Settings
        public float walkSpeed = 6.0f;
        public float sprintSpeed = 8.5f;
    }

    /* Camera Settings */
    public sealed class CameraSettings { //Houses base settings for the player's camera rotation
        //Settings
        [XmlElement("mouse_sensitivity")]
        public Vector2 mouseSensitivity = new Vector2(5.0f, 5.0f);

        [XmlIgnore]
        public float downClamp = 65.0f;

        [XmlIgnore]
        public float upClamp = -65.0f;
    }
}
