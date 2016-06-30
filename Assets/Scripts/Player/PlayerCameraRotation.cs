using UnityEngine;

public sealed class PlayerCameraRotation : MonoBehaviour {
    private float horizontalRotation;
    private float verticalRotation;

    private void Update() {
        HandleRotation();
    }

    private void HandleRotation() { //Handles camera rotation
        //Sets a horizontal rotation based on the X/left and right motions of the mouse
        horizontalRotation = Input.GetAxis("Mouse X") * PlayerManager.Instance.settings.camSettings.mouseSensitivity.x;

        transform.Rotate(new Vector3(0.0f, horizontalRotation, 0.0f)); //Rotates camera on X axis

        verticalRotation -= Input.GetAxis("Mouse Y") * PlayerManager.Instance.settings.camSettings.mouseSensitivity.y; //Sets vertical rotation based on mouse's Y positon
        verticalRotation = Mathf.Clamp(verticalRotation, PlayerManager.Instance.settings.camSettings.upClamp, PlayerManager.Instance.settings.camSettings.downClamp); //Clamps vertical rotation so player can't look up forever
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0.0f, 0.0f); //Actually rotates camera vertically
    }
}