using UnityEngine;

public static class ExtensionMethods {
    /* GameObject Methods */
    public static void EnableColliders(this GameObject gm) { //Enables all colliders on a game object
        Collider[] colliders = gm.GetComponents<Collider>(); //Gets all colliders on game object

        foreach(Collider col in colliders) //Loops through every collider
            col.enabled = true; //Enables all colliders
    }

    public static void DisableColliders(this GameObject gm) { //Disables all colliders on a game object
        Collider[] colliders = gm.GetComponents<Collider>(); //Gets all colliders on game object

        foreach(Collider col in colliders) //Loops through every collider
            col.enabled = false; //Disables all colliders
    }

    public static Vector3 GetCenterBounds(this GameObject gm) { //Returns the Vector3 of the game object's "center" bounds
        Renderer renderer = gm.GetComponent<Renderer>();

        return renderer.bounds.center;
    }

    public static Vector3 GetExtendedBounds(this GameObject gm) { //Returns the Vector3 of the game object's "extended" bounds. Always half of the actual size. Similar to the scale of the game object
        Renderer renderer = gm.GetComponent<Renderer>();

        return renderer.bounds.extents;
    }

    public static float GetHeight(this GameObject gm) { //Returns the height of the game object. Always half of the actual size
        Renderer renderer = gm.GetComponent<Renderer>();

        return renderer.bounds.extents.y; //The height of the object
    }

    public static void SetAudioClip(this GameObject gm, AudioSource audio, AudioClip clip) { //Assign a audio clip to the audio source
        if(audio && clip) //Makes sure the audio source has been assigned and an audio clip has been given
            audio.clip = clip;
    }

    public static void PlayAudio(this GameObject gm, AudioSource audio) { //Plays the audio source
        if(audio) //Makes sure the audio source has been assigned
            if(audio.clip) //Makes the audio source has a clip
                audio.Play();
    }

    public static void PlaySafeAudio(this GameObject gm, AudioSource audio) { //Plays the audio source if it isn't playing currently
        if(audio) //Makes sure the audio source has been assigned
            if(audio.clip) //Makes the audio source has a clip
                if(!audio.isPlaying) //If the audio source isn't playing
                    audio.Play();
    }
    
    public static void StopAudio(this GameObject gm, AudioSource audio) { //Stops playing audio
        if(audio) //Makes sure the audio source has been assigned
            if(audio.isPlaying) //If the audio source is playing something
                audio.Stop();
    }


    /* Transform methods */
    public static float GetHeight(this Transform t) { //Returns the height of the transform. Always half of the actual size. Same as GameObject.GetHeight();
        Renderer renderer = t.GetComponent<Renderer>();

        return renderer.bounds.extents.y; //The height of the transform
    }

    public static Vector3 GetAllBounds(this Transform t) { //Returns all of the "bounds"/dimensions of a game object. Always half of the actual size. Same as GameObject.GetAllBounds();
        Renderer renderer = t.GetComponent<Renderer>();

        return renderer.bounds.extents;
    }

    /* Rigidbody Methods */
    public static void Freeze(this Rigidbody rb) { //Freezes a Rigidbody's positon
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public static void Unfreeze(this Rigidbody rb) { //Unrreezes a Rigidbody's positon
        rb.constraints = RigidbodyConstraints.None;
    }

    public static void StopMovement(this Rigidbody rb) { //Freezes, stop Rigidbody's movement, rotatation, etc, and then unfreezes
        rb.Freeze();
        rb.velocity = Vector3.zero;
        rb.Unfreeze();
    }

    /* Vector3 Methods */
    public static Vector3 CombineDot(this Vector3 v3) { //Combines the Vector3.Dot() method for the X, Y, and Z axis
        if(v3.x > 1.0f || v3.x < -1.0f) //Normalize X axis
            v3.x = v3.normalized.x;
        else if(v3.y > 1.0f || v3.y < -1.0f) //Normalize Y axis
            v3.y = v3.normalized.y;
        else if(v3.z > 1.0f || v3.z < -1.0f) //Normalize Z axis
            v3.z = v3.normalized.z;

        //Get all Dot products
        float horizontalDot = Vector3.Dot(v3, Vector3.right);
        float upDot = Vector3.Dot(v3, Vector3.up);
        float verticalDot = Vector3.Dot(v3, Vector3.forward);

        Vector3 finalPos = new Vector3(horizontalDot, upDot, verticalDot); //Combines all of the Dot products

        return finalPos;
    }
}
