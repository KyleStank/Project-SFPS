using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Networking.Match;

using MLAPI;

public sealed class CustomNetworkManager : NetworkingManager {
    //Singelton
    private static CustomNetworkManager _instance;

    public static CustomNetworkManager Instance {
        get {
            if(_instance == null) { //If this game object doesn't exist
                if(!GameObject.Find("Network Manager")) {
                    GameObject go = new GameObject("Network Manager"); //Create Network manager game object
                    go.AddComponent<CustomNetworkManager>(); //Add class to newly created game object
                }
            }

            return _instance;
        }
    }

    private void Awake() {
        _instance = this; //Singelton creation
    }

    /* Joining */
    public void JoinLANGame() { //Lets the client join the game
        SetIPAddress(UIManager.Instance.mainMenu.ipAddressField.text);
        SetPort(UIManager.Instance.mainMenu.portField.text);
        StartClient();
    }

    public void JoinOnlineGame(UnityEngine.Networking.Types.NetworkID matchID) { //Joins an online game, based on UI
        //Instance.matchMaker.JoinMatch(matchID, "", "", "", 0, 0, Instance.OnMatchJoined); TODO: Look at this.
    }

    /* Hosting */
    public void HostLANGame() { //Lets the client host the game
        SetPort(UIManager.Instance.mainMenu.portField.text);
        StartHost();
    }

    public void HostOnlineGame() { //Hosts/creates an online game, based on UI
        //Instance.matchMaker.CreateMatch(UIManager.Instance.mainMenu.gameNameField.text, Instance.matchSize, true, "", "", "", 0, 0, Instance.OnMatchCreate); TODO: Look at this.
    }
     
    /* Misc */
    private void SetPort(string port) { //Set the port to open(LAN)
        int port_number = System.Convert.ToInt32(port);
        //networkPort = port_number; TODO: Look at this.
    }

    private void SetIPAddress(string ip) { //Set the IP adress to join(LAN)
        //networkAddress = ip; TODO: Look at this.
    }
}
