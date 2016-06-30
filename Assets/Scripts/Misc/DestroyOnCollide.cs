using UnityEngine;

public sealed class DestroyOnCollide : MonoBehaviour {
    public string[] safeTags;
    public float destroyDelay = 0.0f;
    public float afterAwakeDestroyTime = 0.0f;

    private void OnCollisionEnter(Collision col) {
        if(safeTags.Length <= 0) { //If there aren't any safe tags
            Destroy(gameObject, destroyDelay);
        } else {
            foreach(string tag in safeTags) { //Loops through every tag
                if(col.transform.tag == tag)
                    return;
            }

            Destroy(gameObject, destroyDelay);
        }
    }

    private void Awake() {
        if(afterAwakeDestroyTime > 0.0f)
            Destroy(gameObject, afterAwakeDestroyTime);
    }
}
