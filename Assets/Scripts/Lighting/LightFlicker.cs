using System.Collections;
using UnityEngine;

public sealed class LightFlicker : MonoBehaviour {
    //References
    private new Light light;

    //Flickering Variables
    [Range(0.01f, 1.0f)][SerializeField]private float minFlickerSpeed = 0.01f;
    [Range(0.01f, 1.0f)][SerializeField]public float maxFlickerSpeed = 0.2f;
    [Range(1.0f, 10.0f)][SerializeField]public float minIntensity = 1.0f;
    [Range(1.0f, 10.0f)][SerializeField]public float maxIntensity = 4.0f;

    private void Start() {
        light = GetComponentInChildren<Light>(); //Finds light in children

        StartCoroutine(Flicker()); //Starts the flickering timer
    }

    private IEnumerator Flicker() { //Flickers the light's intensity every so often. This HAS to be called in the Start/Awake method, else it will crash the game
        while(true) { //This is what makes the timer run forever. This is also what crashes the game if ran in the Update method
            light.intensity = Random.Range(minIntensity, maxIntensity); //Changes intensity
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed)); //Waits
            light.intensity = Random.Range(minIntensity, maxIntensity); //Changes intensity
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed)); //Waits
        }
    }
}
