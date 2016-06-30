using UnityEngine;

public class Weapon : MonoBehaviour {
    public enum FireType {
        SemiAutomatic,
        Automatic
    }

    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;
    public Transform impactEffect;
    public float damage = 15.0f;
    public int bulletsInMag = 30;
    public int magazines = 3;
    public FireType fireType;
    public float fireRate = 0.1f;
    public float spread = 20f;
    public int bulletsPerShot = 1;
    public float shootDistance = 100.0f;

    //Variables that aren't shown in the inspector
    [HideInInspector]
    public new AudioSource audio;
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public float nextFire = 0.0f;
    [HideInInspector]
    public float newFOV;
    [HideInInspector]
    public float newSpread;
    [HideInInspector]
    public float defaultFOV;
    [HideInInspector]
    public float defaultSpread;

    private int defaultAmmoInMag;

    private void Awake() {
        audio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
            
        //Sets up all of the defaults
        defaultFOV = Camera.main.fieldOfView;
        defaultSpread = spread;
        newFOV = defaultFOV * 0.75f;
        newSpread = defaultSpread / 2.0f;
        defaultAmmoInMag = bulletsInMag;
    }

    public void Reload() { //Reloads the weapon
        if(magazines > 0 && bulletsInMag < defaultAmmoInMag) { //If the weapon needs to be reloaded
            bulletsInMag = defaultAmmoInMag; //Actually reload the weapon
            magazines--;

            //Play reload sound
            if(reloadSound && audio) {
                audio.clip = reloadSound;
                audio.Play();
            }
        }
    }
}
