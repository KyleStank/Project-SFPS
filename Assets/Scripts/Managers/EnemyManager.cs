using UnityEngine;
using System.Xml.Serialization;

[DisallowMultipleComponent]
public sealed class EnemyManager : MonoBehaviour {
    //Singelton
    private static EnemyManager _instance;

    public static EnemyManager Instance {
        get {
            if(_instance == null) { //If this game object doesn't exist
                GameObject go = new GameObject("Enemy Manager"); //Create game manager game object
                go.AddComponent<EnemyManager>(); //Add class to newly created game object
            }

            return _instance;
        }
    }

    /* Enemy Settings */
    public EnemySettings settings = new EnemySettings();

    private void Awake() {
        _instance = this; //Singelton creation
    }
}

[XmlRoot("enemy_settings")]
public sealed class EnemySettings {
    //Lets us access the player's settings
    [XmlElement("movement_settings")]
    public MovementSettings moveSettings = new MovementSettings();

    /* Movement Settings */
    public sealed class MovementSettings { //Houses base settings for the player's movement
        //Settings
        [XmlElement("move_speed")]
        public float moveSpeed = 5.0f;
    }
}
