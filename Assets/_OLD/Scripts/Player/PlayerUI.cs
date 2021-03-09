using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public sealed class PlayerUI : MonoBehaviour {
    private Player player;
    private PlayerWeapon pWeapon;
    
    private void Awake() {
        player = GetComponent<Player>();
        pWeapon = GetComponent<PlayerWeapon>();

        //Add this UI to the UpdateUI event
        UIManager.Instance.OnUIUpdate.AddListener(() => UpdateAmmoUI());
    }
    
    private void Update() {
        //If client wants to pause or unpause the game with the keyboard
        if(player.isLocalPlayer) {
            //Pause Menu
            if(Input.GetKeyDown(KeyCode.Escape)) {
                //Pause or unpause the game
                GameManager.Instance.isGamePaused = !GameManager.Instance.isGamePaused;

                //Open or close the pause menu
                if(GameManager.Instance.isGamePaused)
                    OpenPauseMenu();
                else
                    ClosePauseMenu();
            }

            //Scoreboard
            if(Input.GetKeyDown(KeyCode.Tab)) //Show
                OpenScoreboard();

            if(Input.GetKeyUp(KeyCode.Tab)) //Hide
                CloseScoreboard();
        }
    }

    public void UpdateAmmoUI() { //Update player's ammo UI
        if(player.isLocalPlayer) {
            UIManager.Instance.player.ammoText.text = pWeapon.weapon.bulletsInMag + " / " + pWeapon.weapon.defaultAmmoInMag;
            UIManager.Instance.player.magsText.text = pWeapon.weapon.magazines.ToString();
        }
    }

    /* Pause Menu */
    public void OpenPauseMenu() { //Opens the pause menu
        UIManager.Instance.pauseMenu.pauseMenu.SetActive(true);
        GameManager.Instance.cursor.SetVisible(true);
    }

    public void OpenPauseSettingsMenu() { //Opens the Pause Menu settings menu
        UIManager.Instance.pauseMenu.pauseSettingsMenu.SetActive(true);
    }

    /* Scoreboard */
    public void OpenScoreboard() { //Shows the scoreboard
        UIManager.Instance.scoreboard.scoreboard.SetActive(true);
        Transform parent = UIManager.Instance.scoreboard.scoreboard.transform.Find("Player Scores");

        //Convert the Players list into an array
        Player[] players = PlayerManager.Instance.management.ValuesToArray();

        //Destroy all current scores
        for(int i = 0; i < parent.childCount; i++)
            Destroy(parent.GetChild(i).gameObject);

        //Loop through each player, and show their scores
        for(int i = 0; i < PlayerManager.Instance.management.GetPlayers().Count; i++) {
            //Create the score UI prefab
            GameObject gm = (GameObject)Instantiate(UIManager.Instance.scoreboard.playerScore,
                UIManager.Instance.scoreboard.playerScore.transform.position,
                UIManager.Instance.scoreboard.playerScore.transform.rotation);

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

            //Assign text
            gm.GetComponentInChildren<Text>().text =
                string.Format("{0}          Kills: {1}          Deaths: {2}", players[i].username, players[i].kills, players[i].deaths);
        }
    }

    /* Closing methods */
    public void ClosePauseMenu() { //Closes the pause menu
        //Unpauses the game
        GameManager.Instance.isGamePaused = false;

        //Close UI
        UIManager.Instance.pauseMenu.pauseMenu.SetActive(false);
        GameManager.Instance.cursor.SetVisible(false);
    }

    public void ClosePauseSettingsMenu() { //Closes the Pause Menu settings menu
        UIManager.Instance.pauseMenu.pauseSettingsMenu.SetActive(false);
    }

    public void CloseScoreboard() { //Closes the scoreboard
        UIManager.Instance.scoreboard.scoreboard.SetActive(false);
    }
}
