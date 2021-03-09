using UnityEngine;
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

    /* Non-XML Settings */
    public PlayerManagement management = new PlayerManagement();

    private void Awake() {
        _instance = this; //Singelton creation
    }

    /* Non-XML classes */
    public sealed class PlayerManagement {
        private Dictionary<string, Player> playerList = new Dictionary<string, Player>();
        private const string PLAYER_PREFIX = "Player: ";

        public void RegisterPlayer(string netID, Player p) { //Registers the player to the player list
            //Setup and add player to list
            string playerID = PLAYER_PREFIX + netID;

            playerList.Add(playerID, p);
            p.transform.name = playerID;
        }

        public void DeregisterPlayer(string netID) { //Deregisters the player from the player list
            string playerID = PLAYER_PREFIX + netID;

            if(playerList.ContainsKey(playerID)) //If provided player exists
                //Remove player from the player list
                playerList.Remove(playerID);
        }

        public Dictionary<string, Player> GetPlayers() { //Returns entire dictionary of the player list
            return playerList;
        }

        public Player GetPlayer(string playerID) { //Returns a specific player based on it's ID
            if(playerList.ContainsKey(playerID)) //Check if player exists in the dictionary
                return playerList[playerID];
            else
                return null;
        }

        public Player[] ValuesToArray() { //Turns the values into an array
            //Get all players
            Player[] players = new Player[playerList.Count];

            //Copy values into the array
            playerList.Values.CopyTo(players, 0);

            return players;
        }
    }
}
