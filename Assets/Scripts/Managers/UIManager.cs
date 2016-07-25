using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class UIManager : MonoBehaviour {
    //Singelton
    private static UIManager _instance;

    public static UIManager Instance {
        get {
            if(_instance == null) { //If this game object doesn't exist
                if(!GameObject.Find("UI Manager")) {
                    GameObject go = new GameObject("UI Manager"); //Create UI manager game object
                    go.AddComponent<UIManager>(); //Add class to newly created game object
                }
            }

            return _instance;
        }
    }

    //Lets us access various UI aspects of the game
    public PlayerUI player = new PlayerUI();
    public MainMenuUI mainMenu = new MainMenuUI();
    public PauseMenuUI pauseMenu = new PauseMenuUI();
    public ScoreboardUI scoreboard = new ScoreboardUI();

    //Events
    public UnityEvent OnUIUpdate;

    private void Awake() {
        _instance = this; //Singelton creation

        LoadUI();
    }

    private void Update() {
        OnUIUpdate.Invoke();
    }

    private void LoadUI() { //Loads all of the UI resources
        //Create essential game objects
        GameObject ui = GameObject.Find("UI");
        GameObject menus = (GameObject)Resources.Load("UI/Menus");
        GameObject gm;

        /* Menus */
        //Assign Menus' parents'
        menus = (GameObject)Instantiate(menus, menus.transform.position, menus.transform.rotation);
        menus.transform.SetParent(ui.transform, false);

        /* Player UI */
        //Load main ammo HUD game object
        gm = (GameObject)Resources.Load("UI/Ammo HUD");

        //Assign HUD's parent
        player.ammoUI = (GameObject)Instantiate(gm, gm.transform.position, gm.transform.rotation);
        player.ammoUI.transform.SetParent(ui.transform, false);

        //Correctly assign player's UI
        player.AssignUI();

        /* Main Menu UI */
        //Load main menu HUD game object
        gm = (GameObject)Resources.Load("UI/Main Menu HUD");

        //Assign HUD's parent
        mainMenu.mainMenuUI = (GameObject)Instantiate(gm, gm.transform.position, gm.transform.rotation);
        mainMenu.mainMenuUI.transform.SetParent(ui.transform, false);

        //Assign main menu's "menu"
        mainMenu.mainMenuMenu = menus.transform.Find("Main Menu").gameObject;

        //Load online game info button
        mainMenu.onlineGame = (GameObject)Resources.Load("UI/Online Game");

        //Load username creation menu
        gm = (GameObject)Resources.Load("UI/Username Creation");

        //Create username creation menu
        mainMenu.usernameCreationMenu = (GameObject)Instantiate(gm, gm.transform.position, gm.transform.rotation);
        mainMenu.usernameCreationMenu.transform.SetParent(ui.transform, false);
        mainMenu.usernameCreationMenu.SetActive(true);

        //Load username input field
        mainMenu.usernameField = mainMenu.usernameCreationMenu.transform.Find("Username Input Field").gameObject.GetComponent<InputField>();

        //Correctly assign the menu UI
        mainMenu.AssignUI();

        /* Pause Menu UI */
        //Load pause menu
        gm = (GameObject)Resources.Load("UI/Pause Menu");

        //Create the pause menu
        pauseMenu.pauseMenu = (GameObject)Instantiate(gm, gm.transform.position, gm.transform.rotation);
        pauseMenu.pauseMenu.gameObject.SetActive(false);

        //Assign menu's parent
        pauseMenu.pauseMenu.transform.SetParent(ui.transform, false);

        //Correctly assign the pause menu UI
        pauseMenu.AssignUI();

        /* Scoreboard UI */
        gm = (GameObject)Resources.Load("UI/Scoreboard");

        //Create the scoreboard
        scoreboard.scoreboard = (GameObject)Instantiate(gm, gm.transform.position, gm.transform.rotation);
        scoreboard.scoreboard.gameObject.SetActive(false);

        //Assign the score prefab
        scoreboard.playerScore = (GameObject)Resources.Load("UI/Player Score");

        //Assign scoreboard's parent
        scoreboard.scoreboard.transform.SetParent(ui.transform, false);
    }
    
    public class PlayerUI {
        //Ammo UI
        public GameObject ammoUI;

        public Text ammoText;
        public Text magsText;

        public void AssignUI() { //Assign all of the UI correctly
            if(ammoUI) {
                //Text
                Text[] textComponents = ammoUI.GetComponentsInChildren<Text>();

                foreach(Text text in textComponents) { //Loop through all text components grabbed
                    if(text.name == "Ammo HUD Ammo")
                        ammoText = text;
                    else if(text.name == "Ammo HUD Mags")
                        magsText = text;
                }
            }
        }
    }
    
    public class MainMenuUI {
        public GameObject mainMenuUI;
        public GameObject mainMenuMenu;
        public GameObject usernameCreationMenu;

        public Button joinGameButton;
        public Button hostGameButton;
        public Button settingsButton;
        public Button exitGameButton;

        public InputField portField;
        public InputField ipAddressField;
        public InputField gameNameField;
        public InputField usernameField;

        public GameObject joinGameMenu;
        public GameObject hostGameMenu;
        public GameObject settingsMenu;
        public GameObject exitMenu;

        public GameObject onlineGame;

        public void AssignUI() { //Assign all of the UI correctly
            //Main menu
            if(mainMenuUI) {
                //Buttons
                Button[] btnComponents = mainMenuUI.GetComponentsInChildren<Button>();

                foreach(Button btn in btnComponents) { //Loop through all button components grabbed
                    if(btn.name == "Join Game Button")
                        joinGameButton = btn;
                    else if(btn.name == "Host Game Button")
                        hostGameButton = btn;
                    else if(btn.name == "Settings Button")
                        settingsButton = btn;
                    else if(btn.name == "Exit Button")
                        exitGameButton = btn;
                }
            }

            //Main menu "menu"
            if(mainMenuMenu) {
                //Assign correct menus
                joinGameMenu = mainMenuMenu.transform.Find("Join Game Menu").gameObject;
                hostGameMenu = mainMenuMenu.transform.Find("Host Game Menu").gameObject;
                settingsMenu = mainMenuMenu.transform.Find("Settings Menu").gameObject;
                exitMenu = mainMenuMenu.transform.Find("Exit Menu").gameObject;

                //Input fields
                //Join game field
                GameObject gm = joinGameMenu.transform.Find("LAN Match Section").gameObject;
                ipAddressField = gm.transform.Find("IP Address Field").GetComponent<InputField>();

                //Host game fields
                gm = hostGameMenu.transform.Find("LAN Match Section").gameObject;

                portField = gm.transform.Find("Port Field").GetComponent<InputField>();

                gm = hostGameMenu.transform.Find("Online Match Section").gameObject;

                gameNameField = gm.transform.Find("Game Name Field").GetComponent<InputField>();
            }
        }
    }

    public class PauseMenuUI {
        public GameObject pauseMenu;
        public GameObject pauseSettingsMenu;

        public void AssignUI() { //Assign all of the UI correctly
            if(pauseMenu) {
                pauseSettingsMenu = pauseMenu.transform.Find("Settings Menu").gameObject;
            }
        }
    }
    
    public class ScoreboardUI {
        public GameObject scoreboard;
        public GameObject playerScore;
    }
}
