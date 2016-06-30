using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public sealed class PlayerWeapon : NetworkBehaviour {
    private Weapon weapon;
    private Player player;

    private void Awake() {
        weapon = GetComponentInChildren<Weapon>();
        player = GetComponent<Player>();
    }

    private void Update() {
        print("Ammo in Mag: " + weapon.bulletsInMag + " Magazines: " + weapon.magazines);
        
        //If weapon is semi automatic
        if(weapon.fireType == Weapon.FireType.SemiAutomatic && weapon.bulletsInMag > 0) { //Semi automatic
            if(Input.GetMouseButtonDown(GameManager.Instance.settings.keys.primaryMouseClick) && Time.time > weapon.nextFire)
                Shoot();
        } else if(weapon.fireType == Weapon.FireType.Automatic && weapon.bulletsInMag > 0) { //Fully automatic
            if(Input.GetMouseButton(GameManager.Instance.settings.keys.primaryMouseClick) && Time.time > weapon.nextFire)
                Shoot();
        } else if(weapon.bulletsInMag <= 0 && Input.GetMouseButtonDown(GameManager.Instance.settings.keys.primaryMouseClick)) { //If no ammo is in clip
            //Play the weapon "clicking" noise
            if(weapon.dryFireSound)
                AudioSource.PlayClipAtPoint(weapon.dryFireSound, transform.position); //Play clicking noise
        }

        //If player aims in
        if(Input.GetMouseButton(GameManager.Instance.settings.keys.secondaryMouseClick))
            Aim(true); //Aims in
        else if(Input.GetMouseButtonUp(GameManager.Instance.settings.keys.secondaryMouseClick))
            Aim(false); //Aim out

        //If player reloads weapon
        if(Input.GetKeyDown(KeyCode.R))
            weapon.Reload(); //Reload
    }

    private void Aim(bool aiming) { //Aims down sights, or lower weapon
        weapon.anim.SetBool("Aiming", aiming);

        //Change FOV
        if(aiming) {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, weapon.newFOV, 0.1f);
            weapon.spread = weapon.newSpread;
        } else {
            Camera.main.fieldOfView = weapon.defaultFOV;
            weapon.spread = weapon.defaultSpread;
        }
    }
        
    [Client]
    private void Shoot() { //Shoot the gun
        //Subtract ammo
        weapon.bulletsInMag--;

        //Set the next fire variable
        weapon.nextFire = Time.time + weapon.fireRate; //Adds to "next fire", so player won't be able to shoot really fast
            
        //Play audio
        player.PlayAudioClip(weapon.shootSound, transform.position);

        for(int i = 0; i < weapon.bulletsPerShot; i++) { //Do this for every single bullet shot
            /*Do raycasting. Purpose is for damaging things throughout the world */
            //Handle Rayacst accuracy
            Vector3 rPos = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, Camera.main.nearClipPlane);
            rPos += Random.insideUnitSphere * weapon.spread;

            Ray ray = Camera.main.ScreenPointToRay(rPos);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * weapon.shootDistance);

            if(Physics.Raycast(ray, out hit, weapon.shootDistance)) { //If something was hit
                //If a player was hit
                if(hit.transform.tag == "Player")
                    CmdDamagePlayer(hit.transform.name, weapon.damage);

                //Spawn bullet impact effect for all clients
                CmdSetImpactBulletPosition(hit.normal, hit.point);
            }
        }
    }

    [Command]
    private void CmdDamagePlayer(string playerID, float damage) { //Does damage to the player
        Player player = PlayerManager.Instance.management.GetPlayer(playerID); //Find the player

        if(player.health) //If player has health
            EventManager.RemoveAllAndAddListener(player.health.OnHealthDamage, () => player.health.RpcDamage(damage), true); //Damage the player
    }

    [Command]
    private void CmdSetImpactBulletPosition(Vector3 normalizedPos, Vector3 pos) { //Sets the position for the impact bullet to be spawned
        RpcSpawnImpactBullet(normalizedPos, pos);
    }

    [ClientRpc]
    private void RpcSpawnImpactBullet(Vector3 normalizedPos, Vector3 pos) { //Spawns the bullet impact effect for all clients
        //Impact effect
        //Get Dot product of the normalized hit point
        Vector3 dotProduct = normalizedPos;
        dotProduct = dotProduct.CombineDot();

        //Set position of the bullet
        Vector3 spawnPoint = new Vector3(pos.x + weapon.impactEffect.gameObject.GetExtendedBounds().x * dotProduct.x,
            pos.y + weapon.impactEffect.gameObject.GetExtendedBounds().y * dotProduct.y,
            pos.z + weapon.impactEffect.gameObject.GetExtendedBounds().z * dotProduct.z);

        //Spawn bullet effect
        Transform bullet = (Transform)Instantiate(weapon.impactEffect, spawnPoint, weapon.impactEffect.rotation);

        Destroy(bullet.gameObject, 5.0f); //Destroy after X amount of seconds
    }
}

#region Destructible Environment Handling
                    /*
                    //If a destructible was hit
                    if(hit.transform.tag == GameManager.Instance.settings.tags.destructible || hit.transform.tag == GameManager.Instance.settings.tags.safeRigidbody) {
                        Add force to object
                        Rigidbody rb = hit.transform.GetComponent<Rigidbody>();

                        if(rb) //If rigidbody component is attached to the object that was hit
                            rb.AddForce(weapon.transform.forward, ForceMode.Impulse);

                        Damage the object
                        Misc.Destructible destructible = hit.transform.GetComponent<Misc.Destructible>();
                        FracturedChunk fChunk = hit.transform.GetComponent<FracturedChunk>();

                        if(destructible) { //If Destructible component is attached to the object that was hit
                            destructible.RpcDamage(weapon.damage);
                        }

                        if(fChunk) { //If FractuedChunk component is attached to the hit object
                            if(!fChunk.IsDetachedChunk) //Makes sure the chunk isn't on the ground
                                fChunk.Impact(hit.point, 10.0f, 1.0f, true); //Impact the hit point
                        }
                    }
                    */
#endregion
