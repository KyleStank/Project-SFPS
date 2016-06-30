using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public sealed class EnemyWeapon : MonoBehaviour {
    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public float damage = 25.0f;
    public bool semiAutomatic = true;
    public float fireRate = 0.1f;
    public float spread = 0.1f;
    public int bulletsPerRound = 1;
    public float bulletSpeed = 235.0f;
    public float shootDistance = 100.0f;

    [HideInInspector]public float nextFire = 0.0f;

    public void Shoot() {
        nextFire = Time.time + fireRate; //Adds to "next fire", so player won't be able to shoot really fast

        for(int i = 0; i < bulletsPerRound; i++) { //Do this for every single bullet shot
            Ray ray = new Ray(transform.position, transform.forward + (Random.insideUnitSphere * spread));
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * shootDistance);

            if(Physics.Raycast(ray, out hit, shootDistance)) { //If something is hit
                if(hit.transform.tag == "Player") {
                    Health health = hit.transform.GetComponent<Health>();

                    if(health)
                        health.RpcDamage(damage);
                }
            }
        }
    }
}
