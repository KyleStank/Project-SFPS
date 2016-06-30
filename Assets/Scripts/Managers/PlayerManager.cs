using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Serialization;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class PlayerManager : MonoBehaviour {
    //Singelton
    private static PlayerManager _instance;

    public static PlayerManager Instance {
        get {
            if(_instance == null) { //If this game object doesn't exist
                GameObject go = new GameObject("Player Manager"); //Create game manager game object
                go.AddComponent<PlayerManager>(); //Add class to newly created game object
            }

            return _instance;
        }
    }

    /* Player Settings */
    public PlayerSettings settings = new PlayerSettings();

    /* Non-XML Settings */
    public PlayerManagement management = new PlayerManagement();

    private void Awake() {
        _instance = this; //Singelton creation
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(200, 200, 500, 500));
        GUILayout.BeginVertical();

        foreach(string playerID in management.GetPlayers().Keys) {
            Player p = management.GetPlayer(playerID);
            if(p)
                GUILayout.Label("Name: " + p.transform.name + "    -    " + "ID: " + playerID + "    -    " + "Health: " + p.health.GetHealth() +
                    "    -    " + "Dead?: " + p.health.IsDead());
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    /* Non-XML classes */
    public sealed class PlayerManagement {
        private Dictionary<string, Player> playerList = new Dictionary<string, Player>();
        private const string PLAYER_PREFIX = "Player: ";

        public void RegisterPlayer(string netID, Player player) { //Registers the player to the player list
            string playerID = PLAYER_PREFIX + netID;

            playerList.Add(playerID, player);
            player.transform.name = playerID;
        }

        public void DeregisterPlayer(string netID) { //Deregisters the player from the player list
            string playerID = PLAYER_PREFIX + netID;
            playerList.Remove(playerID);
        }

        public Dictionary<string, Player> GetPlayers() { //Returns entire dictionary of the player list
            return playerList;
        }

        public Player GetPlayer(string playerID) { //Returns a specific player based on it's ID
            return playerList[playerID];
        }
    }
}

[XmlRoot("player_settings")]
public sealed class PlayerSettings {
    //Lets us access the player's settings
    [XmlElement("movement_settings")]
    public MovementSettings moveSettings = new MovementSettings();
    [XmlElement("camera_settings")]
    public CameraSettings camSettings = new CameraSettings();
    [XmlElement("interaction_settings")]
    public InteractionSettings interactSettings = new InteractionSettings();

    /* Movement Settings */
    public sealed class MovementSettings { //Houses base settings for the player's movement
        //Settings
        [XmlElement("move_speed")]
        public float moveSpeed = 6.0f;
        [XmlElement("jump_height")]
        public float jumpHeight = 5.0f;
    }

    /* Camera Settings */
    public sealed class CameraSettings { //Houses base settings for the player's camera rotation
        //Settings
        [XmlElement("mouse_sensitivity")]
        public Vector2 mouseSensitivity = new Vector2(5.0f, 5.0f);
        [XmlElement("look_down_clamp")]
        public float downClamp = 65.0f;
        [XmlElement("look_up_clamp")]
        public float upClamp = -65.0f;
    }

    /* Interaction Settings */
    public sealed class InteractionSettings { //Houses base settings for the player's interactions
        //Settings
        [XmlElement("throw_power")]
        public float throwPower = 20.0f;
        [XmlElement("interaction_distance")]
        public float interactDistance = 3.0f;
        [XmlElement("place_down_distance")]
        public float placeDownDistance = 15.0f;
    }
}
