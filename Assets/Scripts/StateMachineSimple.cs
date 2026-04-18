using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class StateMachineSimple : MonoBehaviour
{
    public enum State { Idle, Patrol, Search, Chase, Investigate }
    [Header("Scene References")]
    public GameObject character;
    public Transform[] waypoints;
    public GameManager gameManager;
    [Header("Config Values")]
    public float idleThreshold = 1.0f;
    public float waypointThreshold = 0.5f;
    public float searchThreshold = 5.0f;
    public float investigateThreshold = 5.0f;
    public float viewRadius = 10f;
    public float viewAngle = 60f;
    public float investigateDistance = 1.5f;


    float reactionDelay = 0.5f;
    float lastSeenTime = -10f;
    float idleTime = 0.0f;
    State state;
    int waypointIndex = 0;
    float searchTime = 0f;
    float investigateTime = 0f;

    NavMeshAgent agent;

    bool viewEnabled = false;
    bool canSeePlayer = false;
    bool reachedLastPosition = false;
    Vector3 lastKnownPlayerPosition;

    bool soundHeard = false;
    Vector3 soundLocation = Vector3.zero;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = State.Idle;
        idleTime = Time.time;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Search:
                Search();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Investigate:
                Investigate();
                break;
        }
    }


    public void SoundTrigger(SoundObject soundObject)
    {
        soundHeard = true;
        soundLocation = soundObject.transform.position;
    }
    //state machines

    void Idle()
    {
        viewEnabled = false;
        float timeElapsed = Time.time - idleTime;
        if (timeElapsed > idleThreshold)
        {
            EnterPatrol();
        }
    }
    void EnterPatrol()
    {
        state = State.Patrol;
        waypointIndex++; //demo video patrolIndex !
        if (waypointIndex >= waypoints.Length) waypointIndex = 0;
    }
    void Patrol()
    {
        Vector3 waypoint = waypoints[waypointIndex].position;
        agent.SetDestination(waypoint);

        viewEnabled = true;
        canSeePlayer = InViewCone();
        if (canSeePlayer)
        {
            lastSeenTime = Time.time;
        }
        //wait for short delay before chasing
        if (Time.time - lastSeenTime < reactionDelay)
        {
            state = State.Chase;
        }
        float distance = Vector3.Distance(transform.position, waypoint);
        if (distance < waypointThreshold)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length) waypointIndex = 0;
            state = State.Idle;
            idleTime = Time.time;
        }
       // if (canSeePlayer)
        //{
          //  state = State.Chase;
        //}
        if (soundHeard)
        {
            EnterInvestigate();
        }

        //float distanceToWaypoint
    }
    void Search()
    {
        //first go to the last known player position
        if (!reachedLastPosition)
        {
            agent.SetDestination(lastKnownPlayerPosition);

            float distance = Vector3.Distance(transform.position, lastKnownPlayerPosition);

            if (distance < 1f)
            {
                reachedLastPosition = true;
            }
        }
        else
        {
            //after reaching start random search
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 5f;
                randomDirection += transform.position;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        //check search duration
        float elapsedSearchTime = Time.time - searchTime;

        if (elapsedSearchTime >= searchThreshold)
        {
            EnterPatrol();
        }

        canSeePlayer = InViewCone();

        if (canSeePlayer)
        {
            state = State.Chase;
        }

        if (soundHeard)
        {
            EnterInvestigate();
        }
    }


    void Chase()
    {
        //when the AI spots and loses sight of a player, instead of randomly wandering around it goes to the last place the player was seen
        lastKnownPlayerPosition = character.transform.position;
        reachedLastPosition = false;

        agent.SetDestination(character.transform.position);
        //catch condition
        float distanceToPlayer = Vector3.Distance(transform.position, character.transform.position);

        if(distanceToPlayer < 1.5f)
        {
            gameManager.GameOver();
        }
        canSeePlayer = InViewCone();
        if (!canSeePlayer)
        {
            state = State.Search;
            searchTime = Time.time;
        }
    }

    void EnterInvestigate()
    {
        state = State.Investigate;
        searchTime = Time.time;
    }
    void Investigate()
    {
        agent.SetDestination(soundLocation);
        float distance = Vector3.Distance(transform.position, soundLocation);
        if (distance <= investigateDistance)
        {
            float timeElapsed = Time.time - investigateTime;

            transform.Rotate(Vector3.up, Mathf.Sin(Time.time) * Time.deltaTime * 45f);
            

            if (timeElapsed >= investigateThreshold)
            
            {
                soundHeard = false;
                EnterPatrol();
            }
        }
        else
        {
            investigateTime = Time.time;
        }

        canSeePlayer = InViewCone();
        if (canSeePlayer)
        {
            soundHeard = false;
            state = State.Chase;
        }
    }
   
    bool InViewCone()
    {
        if (Vector3.Distance(transform.position, character.transform.position) > viewRadius)
            return false;
        Vector3 npcToCharacter = character.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, npcToCharacter) > 0.5f * viewAngle)
            return false;
        Vector3 toCharacterDir = npcToCharacter.normalized;
        if (Physics.Raycast(transform.position,toCharacterDir, out RaycastHit ray, viewRadius))
        {
            return ray.transform == character.transform;
        }
        return false;  
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform waypoint in waypoints)
        {
            Gizmos.DrawWireSphere(waypoint.position, 0.5f);
        }
        if (viewEnabled)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.25f);
            if (canSeePlayer) Handles.color = new Color(1f, 0f, 0f, 0.25f);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, viewAngle / 2, viewRadius);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -viewAngle / 2, viewRadius);

        }
    }

}
