using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public enum AnimalState 
{
    Idle,
    Moving,
    Chase,
}


[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField] private float wanderDistance = 50f;//how far animal can move in one go
    [SerializeField] private float wanderWalkSpeed = 5f;
    [SerializeField] private float wanderMaxTime = 6f;
    [SerializeField] private int maxTimeToCheckCanWalkGround = 5;

    [Header("Idle")]
    [SerializeField] private float idleTime = 6f;

    [Header("Chase")]
    [SerializeField] private float runSpeed = 8;

    [Header("Health")]
    [SerializeField] private float health = 10;

    protected Animator animator; 
    protected NavMeshAgent navMeshAgent;
    protected AnimalState currentState = AnimalState.Idle;

    private void Start()
    {
        InitialiseAnimal();
    }

    protected virtual void InitialiseAnimal()
    {
        animator=transform.GetChild(0).GetChild(0).GetComponent<Animator>(); 
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = wanderWalkSpeed;

        currentState = AnimalState.Idle;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch(currentState)
        {
            case AnimalState.Idle:
                HandleIdleState();
                break;
            case AnimalState.Moving:
                HandleMovingState();
                break;
            case AnimalState.Chase:
                HandleChaseState();
                break;
        }
    }

    protected virtual void CheckChaseConditions()
    {
        
    }

    protected virtual void HandleChaseState()
    {
        StopAllCoroutines();

    }

    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        for (int i = 0; i < maxTimeToCheckCanWalkGround; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance;
            randomDirection += origin;
            NavMeshHit navMehsHit;

            if (NavMesh.SamplePosition(randomDirection, out navMehsHit, distance, NavMesh.AllAreas))
            {
                return navMehsHit.position;
            }
        }

        return origin;
    }

    protected virtual void HandleIdleState()
    {
        StartCoroutine(WaitToMove());
    }

    private IEnumerator WaitToMove()
    {
        float waitTime = Random.Range(idleTime / 2, idleTime * 2);
        yield return new WaitForSeconds(waitTime);

        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);
        navMeshAgent.SetDestination(randomDestination);
        SetState(AnimalState.Moving);

    }

    protected virtual void HandleMovingState()
    {
        StartCoroutine(WaitToReachDestination());
    }

    private IEnumerator WaitToReachDestination()
    {
        float startTime=Time.time;

        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance && navMeshAgent.isActiveAndEnabled) 
        {
            if(Time.time - startTime > wanderMaxTime)
            {
               navMeshAgent.ResetPath();
                SetState(AnimalState.Idle);
               yield break;
            }

            CheckChaseConditions();

            yield return null;
        }

        //destination has been reached
        SetState(AnimalState.Idle);
    }

    protected void SetState(AnimalState newState)
    {
        if(currentState == newState) return;

        currentState = newState;
        OnStateChange(currentState);
    }

    protected virtual void OnStateChange(AnimalState newState)
    {
        animator?.CrossFadeInFixedTime(newState.ToString(), 0.5f);  

        if (newState == AnimalState.Moving)
        {
            navMeshAgent.speed = wanderWalkSpeed;
        }
        if(newState == AnimalState.Chase) 
        {
            navMeshAgent.speed = runSpeed;
        }

        UpdateState();
    }

    public virtual void RecieveDamage(int damge)
    {
        health -= damge;
        if(health < 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
