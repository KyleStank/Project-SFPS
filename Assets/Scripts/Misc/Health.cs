using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
    //Events
    public UnityEvent OnHealthDamage;
    public UnityEvent OnKilled;
    public UnityEvent OnRespawn;
    
    [SerializeField]
    [SyncVar]
    private float health = 100.0f;
    [SerializeField]
    private bool canRespawn = false;
    private float defaultHealth;
    [SerializeField]
    private float destroyTime = 5.0f;

    //Variables that aren't shown in the inspector
    [SyncVar]
    private bool dead = false;

    public void SetHealth(float health, bool affectDeadState = false) { //Sets the health of object
        this.health = health;

        if(affectDeadState && health > 0) //If dead variable is affected
            dead = false;
    }

    public float GetHealth() { //Returns the remaining health of object
        return health;
    }

    public bool CanRespawn() { //Tells us if we are or aren't allowed to respawn
        return canRespawn;
    }

    public float GetDefaultHealth() { //Returns the default amount of health
        return defaultHealth;
    }

    public void SetDestroyTime(float time) { //Sets amount of seconds to wait before destroying object
        destroyTime = time;
    }

    public float GetDestroyTime() { //Returns seconds to wait before object gets destroyed
        return destroyTime;
    }

    public bool IsDead() { //Returns the living state of the object
        return dead;
    }

    private void Awake() {
        defaultHealth = health;
    }

    [ClientRpc]
    public virtual void RpcDamage(float damage) { //Damages the object
        if(!dead) { //If object isn't already dead
            health -= damage;

            if(health <= 0) //If enemy has no more health
                OnKilled.Invoke(); //Destroy this game object
        }
    }
        
    public virtual void Kill() { //"Kills"/destroys the object
        if(!dead) { //Makes sure object isn't already dead/destroyed
            dead = true;

            /* Play death animation */

            if(canRespawn) //If object is allowed to repspawn
                OnRespawn.Invoke(); //Respawn object
        }
    }

    public void Respawn() { //Respawns the object
        StartCoroutine(PlayerManager.Instance.management.GetPlayer(transform.name).Respawn());
    }
}
