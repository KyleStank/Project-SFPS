using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public sealed class PlayerMovement : MonoBehaviour {
    private Rigidbody rb;
    private float horizontalMovement;
    private float forwardMovement;
    private float verticalMovement;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        HandleMovement();
    }

    private void HandleMovement() { //Handles movement
        horizontalMovement = Input.GetAxis("Horizontal"); //Gets horizontal movement of player when key is pressed
        forwardMovement = Input.GetAxis("Vertical"); //Get vertical movement of player when key is pressed

        Vector3 movement = new Vector3(horizontalMovement, verticalMovement, forwardMovement); //Sets up movement variables
        movement = transform.rotation * movement; //Makes sure player rotates correctly when the camera rotates too

        Vector3 velocity = new Vector3(movement.x * PlayerManager.Instance.settings.moveSettings.moveSpeed, rb.velocity.y, movement.z * PlayerManager.Instance.settings.moveSettings.moveSpeed); //Sets the variable used for the player's velocity
        rb.velocity = velocity; //Sets the player's velocity and moves the player
    }
}
