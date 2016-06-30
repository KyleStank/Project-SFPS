using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public sealed class EnemyHealth : MonoBehaviour {
    public float health = 100.0f;
    public string deathSound = "enemy_death2";
    public string hitMarkerSound = "hitmarker";

    private AudioSource[] sources;
    private new AudioSource audio;
    private bool dead = false;

    private void Awake() {
        sources = GetComponents<AudioSource>();
    }

    private void SetAudioSource(string clipName) { //Finds the correct audio source
        foreach(AudioSource source in sources) { //Loops through every audio source
            if(source.clip) { //If audio source has a clip
                if(source.clip.name == clipName) { //If correct clip has been found, assign the audio source and stop looking
                    audio = source;
                    return;
                }
            }
        }
    }

    public void Damage(float damage) { //Damages the enemy
        health -= damage;

        //Play audio
        SetAudioSource(hitMarkerSound);

        if(audio.clip) //If audio source has a clip
            if(!audio.isPlaying) //Makes sure audio isn't already playing
                audio.Play();

        if(health <= 0) { //If enemy has no more health
            Kill(); //Kill
            dead = true;
        }
    }

    private void Kill() { //"Kills"/destroys the enemy
        SetAudioSource(deathSound);

        Transform[] transforms = GetComponents<Transform>();
        foreach(Transform t in transforms) {
            if(t.tag != gameObject.tag)
                t.gameObject.SetActive(false);
        }

        if(audio.clip) { //If audio source has a clip
            /* Play death animation */
            audio.Play();

            Destroy(gameObject, audio.clip.length); //Destroy game object after sound is done playing
        } else
            Destroy(gameObject); //Destroy game object instantly
    }
}
