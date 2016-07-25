using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public sealed class PlayerMovement : MonoBehaviour {
    private Player player;
    private Rigidbody rb;
    private float currentSpeed;
    private float horizontalMovement;
    private float forwardMovement;

    private void Awake() {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if(!GameManager.Instance.isGamePaused && !player.health.IsDead()) { //If game isn't paused
            HandleMovement();
        }
    }

    private void HandleMovement() { //Handles movement
        //Base movement
        horizontalMovement = Input.GetAxis("Horizontal"); //Gets horizontal movement of player when key is pressed
        forwardMovement = Input.GetAxis("Vertical"); //Get vertical movement of player when key is pressed

        //Handle sprinting
        if(horizontalMovement != 0.0f || forwardMovement != 0.0f) { //If player is moving
            if(Input.GetKey(GameManager.Instance.settings.keys.sprintKey))
                currentSpeed = GameManager.Instance.settings.moveSettings.sprintSpeed;
            else
                currentSpeed = GameManager.Instance.settings.moveSettings.walkSpeed;
        }

        Vector3 movement = new Vector3(horizontalMovement, 0.0f, forwardMovement); //Sets up movement variables
        movement = transform.rotation * movement; //Makes sure player rotates correctly when the camera rotates too

        Vector3 velocity = new Vector3(movement.x * currentSpeed, rb.velocity.y, movement.z * currentSpeed); //Sets the variable used for the player's velocity
        rb.velocity = velocity; //Sets the player's velocity and moves the player
    }
}
