using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public sealed class EnemyMovement : MonoBehaviour {
    public Transform target; //Rotates towards this
    public Transform targetHolder;
    public EnemyWeapon weapon;
    public int currentWaypoint = 0; //If 1 is used, 1 must be subtracted from this variable when used with arrays
    public float waypointSwitchTime = 10.0f;
    public float lookDistance = 10.0f;
    public float lookSpeed = 25.0f;

    private Transform[] targets;
    private NavMeshAgent agent;
    private Health health;
    private bool isWalking;

    private void Awake() {
        targets = targetHolder.GetComponentsInChildren<Transform>();
        agent = GetComponent<NavMeshAgent>();
        health = target.GetComponent<Health>();

        for(int i = 0; i < targets.Length; i++) { //Loops through the length of target
            if(Vector3.Distance(transform.position, targets[i].position) <= 0.1f) { //If the current position is close to a waypoint
                transform.position = targets[i].position; //Set the position
                currentWaypoint = (i - 1); //Set the waypoint. 1 is subtracted because the length return 1 to 5, but the array works from 0 to 4
            }
        }
    }

    private void Start() {
        StartCoroutine(WaypointTimer(waypointSwitchTime));
    }

    private void Update() {
        //Check if the enemy is walking towards a waypoint or not
        if(Vector3.Distance(transform.position, targets[currentWaypoint].position) <= 0.05f)
            isWalking = false;
        else
            isWalking = true;

        Debug.DrawRay(transform.position, transform.forward * lookDistance);
            
        //Check if the target is close enough, then make it rotate towards this object
        if(Vector3.Distance(transform.position, target.position) <= lookDistance && !health.IsDead()) {
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z); //Rotate the target only in the Y position

            transform.LookAt(targetPos); //Even though this transform is used, the target is still the one being rotated towards this transform

            if(Time.time > weapon.nextFire) //If enemy can shoot
                weapon.Shoot();
        }
    }

    private void ChangeWaypoint() { //Randomly chooses a new waypoint
        currentWaypoint = Random.Range(0, targets.Length); //Randomly choose a new waypoint

        agent.SetDestination(targets[currentWaypoint].position); //Makes the enemy move towards a waypoint
    }

    private IEnumerator WaypointTimer(float time) { //Handles the waypoint system
        while(true) { //Makes sure that this coroutine is always running
            if(!isWalking) { //If the enemy isn't walking
                yield return new WaitForSeconds(time); //Waits for X amount of seconds
                ChangeWaypoint(); //Change the current waypoint
            } else
                yield return null;
        }
    }
}
