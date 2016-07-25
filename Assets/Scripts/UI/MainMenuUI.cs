using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
    private void Start() {
        //Makes sure that the user hasn't already chosen their username
        if(GameManager.Instance.settings.isInitialStartup) {
            CloseMainMenuUI();
            OpenUsernameCreationUI();
        } else {
            OpenMainMenuUI();
            CloseUsernameCreationUI();
        }
    }

    /* Misc UI */
    public void OpenMainMenuUI() { //Opens the main menu's main UI
        UIManager.Instance.mainMenu.mainMenuUI.SetActive(true);
    }

    public void CloseMainMenuUI() { //Closes the main menu's main UI
        UIManager.Instance.mainMenu.mainMenuUI.SetActive(false);
    }

    public void OpenUsernameCreationUI() { //Opens the username creation menu
        UIManager.Instance.mainMenu.usernameCreationMenu.SetActive(true);
    }
    
    public void CloseUsernameCreationUI() { //Closes the username creation menu
        UIManager.Instance.mainMenu.usernameCreationMenu.SetActive(false);
    }
    
    /* Username Creation */
    public void CreateUsername() { //Creates the user's username
        GameManager.Instance.settings.username = UIManager.Instance.mainMenu.usernameField.text;

        //Close username creation menu and show main menu UI
        CloseUsernameCreationUI();
        OpenMainMenuUI();

        //Makes sure that inital setup is finished
        GameManager.Instance.settings.isInitialStartup = false;

        //Save file
        Serialization.SaveXML(GameManager.Instance.settings, Paths.localPath, Paths.Files.gameSettings);
    }

    /* Joining */
    private void UpdateOnlineGamesList() { //Updates the available games list
        if(CustomNetworkManager.Instance.matchInfo == null) {
            if(CustomNetworkManager.Instance.matches != null) {
                //Parent of the game info button
                Transform parent = UIManager.Instance.mainMenu.joinGameMenu.transform.Find("Online Match Section")
                    .transform.Find("Online Games List")
                    .transform.Find("Button Holder");

                //If no matches are going
                if(CustomNetworkManager.Instance.matches.Count <= 0) {
                    for(int i = 0; i < parent.childCount; i++) {
                        Transform t = parent.GetChild(i);

                        Destroy(t.gameObject);
                    }
                    
                    return;
                }

                for(int i = 0; i < CustomNetworkManager.Instance.matches.Count; i++) { //Loops through all matches
                    var match = CustomNetworkManager.Instance.matches[i];
                    
                    //Create the UI match prefab
                    GameObject gm = (GameObject)Instantiate(UIManager.Instance.mainMenu.onlineGame,
                        UIManager.Instance.mainMenu.onlineGame.transform.position,
                        UIManager.Instance.mainMenu.onlineGame.transform.rotation);

                    //Set parent
                    gm.transform.SetParent(parent, false);

                    //Set correct position
                    if(i != 0) {
                        RectTransform rectTransform = gm.GetComponent<RectTransform>();

                        //Set position
                        gm.transform.position = new Vector3(gm.transform.position.x,
                            gm.transform.position.y - (rectTransform.rect.height * i),
                            gm.transform.position.z);
                    }

                    //Set name of object and display the match name
                    gm.name = match.name;
                    gm.GetComponentInChildren<Text>().text = match.name;

                    //Check if this game is already being displayed
                    for(int x = 0; x < parent.childCount; x++) { //Loop through all children
                        Transform t = parent.GetChild(x);

                        if(x != 0) //Makes sure that at least one instance of the match is being displayed
                            if(t.name == match.name) //If there is another instance, destroy it, and stop looping through
                                Destroy(gm);
                    }
                    //if(parent.Find(match.name) != null)
                    //    Destroy(gm);

                    //Assign functionality to the button
                    Button btn = gm.GetComponentInChildren<Button>();

                    //Add functionality to the button
                    btn.onClick.AddListener(() => JoinOnlineGame(match.networkId));
                    btn.onClick.AddListener(() => CloseJoinGameMenu());
                    btn.onClick.AddListener(() => CloseMainMenuUI());
                }
            }
        }
    }

    public void OpenJoinGameMenu() { //Opens "Join Game" menu
        CustomNetworkManager.Instance.StartMatchMaker();

        UIManager.Instance.mainMenu.joinGameMenu.SetActive(true);

        //Lets us access all of the games' info
        CustomNetworkManager.Instance.matchMaker.ListMatches(0, 20, "", false, 0, 0, CustomNetworkManager.Instance.OnMatchList);

        UIManager.Instance.OnUIUpdate.AddListener(() => UpdateOnlineGamesList());
    }

    public void JoinLANGame() { //Joins the game, based on UI fields
        CustomNetworkManager.Instance.JoinLANGame();
    }

    public void JoinOnlineGame(UnityEngine.Networking.Types.NetworkID matchID) { //Joins an online game
        CustomNetworkManager.Instance.JoinOnlineGame(matchID);
    }

    /* Hosting */
    public void OpenHostGameMenu() { //Opens "Host Game" menu
        CustomNetworkManager.Instance.StartMatchMaker();
        UIManager.Instance.mainMenu.hostGameMenu.SetActive(true);
    }

    public void HostLANGame() { //Hosts the game
        CustomNetworkManager.Instance.HostLANGame();
    }

    public void HostOnlineGame() { //Hosts/create an online game
        CustomNetworkManager.Instance.HostOnlineGame();
    }

    /* Settings */
    public void OpenSettingsMenu() { //Opens "Settings" menu
        UIManager.Instance.mainMenu.settingsMenu.SetActive(true);
    }

    /* Exit */
    public void OpenExitGameMenu() { //Opens "Exit" menu
        UIManager.Instance.mainMenu.exitMenu.SetActive(true);
    }

    public void ExitGame() { //Exits the game
        Application.Quit();
    }

    /* Closing methods */
    public void CloseJoinGameMenu() { //Closes "Join Game" menu
        UIManager.Instance.mainMenu.joinGameMenu.SetActive(false);
        UIManager.Instance.OnUIUpdate.RemoveListener(() => UpdateOnlineGamesList());
    }

    public void CloseHostGameMenu() { //Closes "Host Game" menu
        UIManager.Instance.mainMenu.hostGameMenu.SetActive(false);
    }

    public void CloseSettingsMenu() { //Closes "Settings" menu
        UIManager.Instance.mainMenu.settingsMenu.SetActive(false);
    }

    public void CloseExitMenu() { //Closes "Exit" menu
        UIManager.Instance.mainMenu.exitMenu.SetActive(false);
    }
}
