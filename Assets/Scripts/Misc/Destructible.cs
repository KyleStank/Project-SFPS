using UnityEngine;

[RequireComponent(typeof(FracturedObject))]
public sealed class Destructible : Health {
    public FracturedObject fracturedObj;
    public float minImpactForce;

    private void Awake() {
        fracturedObj = GetComponent<FracturedObject>();

        minImpactForce = fracturedObj.EventDetachMinVelocity;
    }

    private void OnCollisionEnter(Collision col) {
        float magnitude = col.relativeVelocity.magnitude;

        if(magnitude >= minImpactForce) //If the collision was strong enough
            RpcDamage(magnitude);
    }

    public override void Kill() { //Destroys the game object
        Collider[] colliders = GetComponents<Collider>();

        foreach(Collider col in colliders) {
            Destroy(col);
        }
    }
}