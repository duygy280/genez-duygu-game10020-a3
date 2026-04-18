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
    public float viewRadius = 15f;
    public float viewAngle = 100f;
    public float investigateDistance = 1.5f;

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

    void Idle()
    {
        viewEnabled = false;

        if (Time.time - idleTime > idleThreshold)
        {
            EnterPatrol();
        }
    }

    void EnterPatrol()
    {
        state = State.Patrol;
        waypointIndex++;
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
            state = State.Chase;
            return;
        }

        float distance = Vector3.Distance(transform.position, waypoint);
        if (distance < waypointThreshold)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length) waypointIndex = 0;

            state = State.Idle;
            idleTime = Time.time;
        }

        if (soundHeard)
        {
            EnterInvestigate();
        }
    }

    void Search()
    {
        if (!reachedLastPosition)
        {
            agent.SetDestination(lastKnownPlayerPosition);

            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
            {
                reachedLastPosition = true;
            }
        }
        else
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = lastKnownPlayerPosition + Random.insideUnitSphere * 5f;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        if (Time.time - searchTime >= searchThreshold)
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
        lastKnownPlayerPosition = character.transform.position;
        reachedLastPosition = false;
        agent.SetDestination(character.transform.position);

        if (Vector3.Distance(transform.position, character.transform.position) < 1.5f)
        {
            gameManager.GameOver();

        }

        canSeePlayer = InViewCone();
        if (!canSeePlayer)
        {
            EnterSearch();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.GameOver();
        }
    }

    void EnterSearch()
    {
        state = State.Search;
        searchTime = Time.time;
        reachedLastPosition = false;
    }

    void EnterInvestigate()
    {
        state = State.Investigate;
        investigateTime = Time.time;
    }

    void Investigate()
    {
        agent.SetDestination(soundLocation);

        float distance = Vector3.Distance(transform.position, soundLocation);

        if (distance <= investigateDistance)
        {
            transform.Rotate(Vector3.up, Mathf.Sin(Time.time) * Time.deltaTime * 45f);

            if (Time.time - investigateTime >= investigateThreshold)
            {
                soundHeard = false;
                EnterSearch();
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
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = character.transform.position + Vector3.up * 1f;

        float distance = Vector3.Distance(origin, target);
        if (distance > viewRadius)
            return false;

        Vector3 direction = (target - origin).normalized;

        if (Vector3.Angle(transform.forward, direction) > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, viewRadius))
        {
            return hit.transform == character.transform || hit.transform.root == character.transform;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (waypoints != null)
        {
            foreach (Transform waypoint in waypoints)
            {
                Gizmos.DrawWireSphere(waypoint.position, 0.5f);
            }
        }

        if (viewEnabled)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.25f);
            if (canSeePlayer)
                Handles.color = new Color(1f, 0f, 0f, 0.25f);

            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, viewAngle / 2, viewRadius);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -viewAngle / 2, viewRadius);
        }
    }
}
