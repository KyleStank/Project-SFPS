﻿using UnityEngine;
using UnityEngine.Events;

using MLAPI;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class Health : NetworkedBehaviour {
    //Events
    public UnityEvent OnHealthDamage;
    public UnityEvent OnKilled;
    public UnityEvent OnRespawn;

    private Player player;

    [SerializeField]
    //[SyncVar] TODO: Look at this.
    private float health = 100.0f;
    [SerializeField]
    private bool canRespawn = false;
    private float defaultHealth;
    [SerializeField]
    private float destroyTime = 5.0f;

    //Variables that aren't shown in the inspector
    //[SyncVar] TODO: Look at this.
    private bool dead = false;

    private void Awake() {
        player = GetComponent<Player>();

        defaultHealth = health;
    }

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

    //[ClientRpc] TODO: Look at this.
    public virtual void RpcDamage(float damage) { //Damages the object
        if(!dead) { //If object isn't already dead
            health -= damage;

            if(health <= 0) { //If enemy has no more health
                OnKilled.Invoke(); //Destroy this game object
            }
        }
    }
        
    public virtual void Kill(Player p) { //"Kills"/destroys the object
        if(!dead && p) { //Makes sure object isn't already dead/destroyed
            //Add a death to this player
            dead = true;
            player.deaths++;

            //Add a kill to the player that killed this player
            p.kills++;

            /* Play death animation */

            if(canRespawn) //If object is allowed to repspawn
                OnRespawn.Invoke(); //Respawn object
        }
    }

    public void Respawn() { //Respawns the object
        StartCoroutine(PlayerManager.Instance.management.GetPlayer(transform.name).Respawn());
    }
}
