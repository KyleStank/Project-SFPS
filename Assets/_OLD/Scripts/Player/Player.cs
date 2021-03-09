using System.Collections;

using UnityEngine;

using MLAPI;

[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
public sealed class Player : NetworkedBehaviour {
    [HideInInspector]
    public Health health;

    [HideInInspector]
    //[SyncVar] TODO: Look at this.
    public string username;
    [HideInInspector]
    //[SyncVar] TODO: Look at this.
    public int kills;
    [HideInInspector]
    //[SyncVar] TODO: Look at this.
    public int deaths;

    [SerializeField]
    [Tooltip("Components that will be toggled when the player spawns into the game.")]
    private Behaviour[] setupComponents;
    [SerializeField]
    [Tooltip("List of AudioClips that will be used for the player. These are ALL synced across the network, meaning all clients will hear them.")]
    private AudioClip[] audioClips;
    [SerializeField]
    [Tooltip("All of the camera's that will change their rendering layers, based on whether or not the player is local.")]
    private Camera[] camerasToChange;
    private string netIDValue;

    // TODO: Look at this.
    //public override void OnStartClient() {
    //    base.OnStartClient();
        
    //    netIDValue = netId.Value.ToString();
    //    PlayerManager.Instance.management.RegisterPlayer(netIDValue, this);
    //}

    // TODO: Look at this.
    //public override void OnStartLocalPlayer() {
    //    base.OnStartLocalPlayer();
    //    CmdSetUsername(GameManager.Instance.settings.username);
    //}

    private void Awake() {
        health = GetComponent<Health>();

        foreach(AudioClip clip in audioClips) //Loop through all audio clips
            GameManager.Instance.AddAudio(clip.name, clip);
    }

    private void Start() {
        Setup();
    }

    private void Setup() { //Performs the player's setup
        if(isLocalPlayer) //If this player is currently the local player
            SetDefaults();
        else
            ToggleComponents(setupComponents, false);
    }

    private void SetDefaults() { //Sets up the defaults for the player
        GameManager.Instance.ToggleSceneCamera(false);
        health.SetHealth(health.GetDefaultHealth(), true);
    }

    private void OnDisable() {
        if(isLocalPlayer) //If this player is currently the local player
            GameManager.Instance.ToggleSceneCamera(true);

        //Deregisters/removes the player from the game's network
        PlayerManager.Instance.management.DeregisterPlayer(netIDValue);
    }
        
    public IEnumerator Respawn() { //Respawns the player
        yield return new WaitForSeconds(GameManager.Instance.matchSettings.respawnDelay);

        //Set position of spawn
        // TODO: Look at this.
        //Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        //transform.position = spawnPoint.position;
        //transform.rotation = spawnPoint.rotation;

        SetDefaults(); //Set up the defaults of the player
    }

    public void DisconnectFromGame() { //Disconnects the player from the game
        CustomNetworkManager.Instance.StopHost();
    }

    /* Helpful Methods */
    public void ToggleComponents(Behaviour[] components, bool active) { //Toggles all of the specified components
        for(int i = 0; i < components.Length; i++) //Loop through the length of the components array given
            components[i].enabled = active; //Enable or disable component accordingly
    }

    public void PlayAudioClip(AudioClip clip, Vector3 position) { //Plays an audio clip for all the clients to hear
        if(clip) { //If the AudioClip exists
            GameManager.Instance.AddAudio(clip.name, clip); //Adds audio to list if it doesn't already exist
            CmdSendAudioToClients(clip.name, position); //Sends audio to the server
        }
    }

    //[Command] TODO: Look at this.
    private void CmdSendAudioToClients(string clipID, Vector3 position) { //Sends audio to the clients
        RpcPlayAudioOnAllClients(clipID, position);
    }

    //[ClientRpc] TODO: Look at this.
    private void RpcPlayAudioOnAllClients(string clipID, Vector3 position) { //Plays audio for everyone
        AudioSource.PlayClipAtPoint(GameManager.Instance.GetAudioClip(clipID), position);
    }

    //[Command] TODO: Look at this.
    private void CmdSetUsername(string value) { //Sets the username of the player
        username = value;
    }
}
