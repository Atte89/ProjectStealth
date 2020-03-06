using UnityEngine;
using System.Collections;
using System.Linq;

public class AgressiveEnemyAI : MonoBehaviour
{
    public float patrolSpeed = 2f;                          // The nav mesh agent's speed when patrolling.
    public float chaseSpeed = 5f;                           // The nav mesh agent's speed when chasing.
    public float chaseWaitTime = 5f;                        // The amount of time to wait when the last sighting is reached.
    public float patrolWaitTime = 1f;                       // The amount of time to wait when the patrol way point is reached.
    public GameObject[] upperPatrolWaypoints;				// An array of game objects for the upper patrol route.
    public GameObject[] lowerPatrolWaypoints;               // An array of game objects for the lower patrol route.
    public GameObject[] transitionWaypoints;                // An array of game objects for transitioning between lower and upper areas.
    public GameObject startTransitionToLower;
    public GameObject startTransitionToUpper;

    private DoneEnemySight enemySight;                      // Reference to the EnemySight script.
    private UnityEngine.AI.NavMeshAgent nav;                // Reference to the nav mesh agent.
    private Transform player;                               // Reference to the player's transform.
    private DonePlayerHealth playerHealth;                  // Reference to the PlayerHealth script.
    private DoneLastPlayerSighting lastPlayerSighting;      // Reference to the last global sighting of the player.
    private float chaseTimer;                               // A timer for the chaseWaitTime.
    private float patrolTimer;                              // A timer for the patrolWaitTime.
    private int wayPointIndex;                              // A counter for the way point array.
    private GameObject currentWaypoint;                     // Current waypoint
    private GameObject lastWaypoint;                        // Previous waypoint visited
    private GameObject[] currentWaypoints;
    private bool doTransition;
    private GameObject[] transitioningTo;


    void Awake()
    {
        // Setting up the references.
        enemySight = GetComponent<DoneEnemySight>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag(DoneTags.player).transform;
        playerHealth = player.GetComponent<DonePlayerHealth>();
        lastPlayerSighting = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneLastPlayerSighting>();
        doTransition = false;
    }


    void Update()
    {
        // If the player is in sight and is alive...
        if (enemySight.playerInSight && playerHealth.health > 0f)
            // ... shoot.
            Shooting();

        // If the player has been sighted and isn't dead...
        else if (enemySight.personalLastSighting != lastPlayerSighting.resetPosition && playerHealth.health > 0f)
            // ... chase.
            Chasing();

        else if (doTransition)
            Transition();

        // Otherwise...
        else
            // ... patrol.
            Patrolling();
    }


    void Shooting()
    {
        // Stop the enemy where it is.
        nav.isStopped = true;
    }


    void Chasing()
    {
        // Create a vector from the enemy to the last sighting of the player.
        Vector3 sightingDeltaPos = enemySight.personalLastSighting - transform.position;

        // If the the last personal sighting of the player is not close...
        if (sightingDeltaPos.sqrMagnitude > 4f)
            // ... set the destination for the NavMeshAgent to the last personal sighting of the player.
            nav.destination = enemySight.personalLastSighting;

        // Set the appropriate speed for the NavMeshAgent.
        nav.isStopped = false;
        nav.speed = chaseSpeed;

        // If near the last personal sighting...
        if (nav.remainingDistance < nav.stoppingDistance)
        {
            // ... increment the timer.
            chaseTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (chaseTimer >= chaseWaitTime)
            {
                // ... reset last global sighting, the last personal sighting and the timer.
                lastPlayerSighting.position = lastPlayerSighting.resetPosition;
                enemySight.personalLastSighting = lastPlayerSighting.resetPosition;
                chaseTimer = 0f;
            }
        }
        else
            // If not near the last sighting personal sighting of the player, reset the timer.
            chaseTimer = 0f;
    }


    void Patrolling()
    {
        // Set an appropriate speed for the NavMeshAgent.
        nav.isStopped = false;
        nav.speed = patrolSpeed;

        // If near the next waypoint or there is no destination...
        if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance)
        {
            // ... increment the timer.
            patrolTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (patrolTimer >= patrolWaitTime)
            {
                if (currentWaypoint == null)
                {
                    currentWaypoint = GetClosestWaypoint();

                    if (upperPatrolWaypoints.Contains(currentWaypoint))
                    {
                        currentWaypoints = upperPatrolWaypoints;
                    }
                    else
                    {
                        currentWaypoints = lowerPatrolWaypoints;
                    }
                }
                else
                {
                    lastWaypoint = currentWaypoint;
                    GameObject[] linkedWaypoints = currentWaypoint.GetComponent<DoneWayPointGizmo>().linkedWaypoints;
                    currentWaypoint = linkedWaypoints[Random.Range(0, linkedWaypoints.Length)];
                    while (!currentWaypoints.Contains(currentWaypoint))
                    {
                        currentWaypoint = linkedWaypoints[Random.Range(0, linkedWaypoints.Length)];
                    }
                    
                }
                // Reset the timer.
                patrolTimer = 0;
            }
        }
        else
            // If not near a destination, reset the timer.
            patrolTimer = 0;

        // Set the destination to the patrolWayPoint.
        if (currentWaypoint != null) nav.destination = currentWaypoint.transform.position;

        if (currentWaypoints != null)
        {
            GameObject waypointClosestToPlayer = lastPlayerSighting.WaypointClosestToPlayer();

            if (!currentWaypoints.Contains(waypointClosestToPlayer) && !transitionWaypoints.Contains(waypointClosestToPlayer))
            {
                doTransition = true;
                lastWaypoint = currentWaypoint;
                currentWaypoints = transitionWaypoints;

                if(upperPatrolWaypoints.Contains(lastWaypoint))
                {
                    currentWaypoint = startTransitionToLower;
                    transitioningTo = lowerPatrolWaypoints;
                }
                else
                {
                    currentWaypoint = startTransitionToUpper;
                    transitioningTo = upperPatrolWaypoints;
                }

                return;
            }
        }
    }

    void Transition()
    {
        // Set an appropriate speed for the NavMeshAgent.
        nav.isStopped = false;
        nav.speed = patrolSpeed;

        // If near the next waypoint or there is no destination...
        if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance)
        {
            // ... increment the timer.
            patrolTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (patrolTimer >= patrolWaitTime)
            {
                GameObject[] linkedWaypoints = currentWaypoint.GetComponent<DoneWayPointGizmo>().linkedWaypoints;

                foreach(GameObject waypoint in linkedWaypoints)
                {
                    if(waypoint != lastWaypoint || transitioningTo.Contains(waypoint))
                    {
                        lastWaypoint = currentWaypoint;
                        currentWaypoint = waypoint;
                        break;
                    }
                }

                // Reset the timer.
                patrolTimer = 0;

                if (!currentWaypoints.Contains(currentWaypoint))
                {
                    if (upperPatrolWaypoints.Contains(currentWaypoint))
                    {
                        currentWaypoints = upperPatrolWaypoints;
                    }
                    else
                    {
                        currentWaypoints = lowerPatrolWaypoints;
                    }

                    doTransition = false;
                    return;
                }
            }
        }
        else
            // If not near a destination, reset the timer.
            patrolTimer = 0;

        // Set the destination to the patrolWayPoint.
        if (currentWaypoint != null) nav.destination = currentWaypoint.transform.position;
    }

    // Calculates the closest waypoint
    GameObject GetClosestWaypoint()
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        GameObject[][] waypointsArray = new GameObject[][] { upperPatrolWaypoints, lowerPatrolWaypoints };

        if (doTransition)
        {
            waypointsArray = new GameObject[][] { transitionWaypoints };
        }

        foreach (GameObject[] waypoints in waypointsArray)
        {
            foreach (GameObject potentialTarget in waypoints)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }
        }
        return bestTarget;
    }
}
