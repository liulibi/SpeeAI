using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prey : Animal
{
    [Header("Prey Variables")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float escapeMaxDistance = 100f;

    private Predator currentPredator = null;

    public void AlterPrey(Predator predator)
    {
        SetState(AnimalState.Chase);
        currentPredator = predator;
        StartCoroutine(RunFromPredator());
    }

    private IEnumerator RunFromPredator()
    {
        //wait until the predator is with in detection range
        while (currentPredator == null || Vector3.Distance(transform.position, currentPredator.transform.position) > detectionRange) 
        {
            yield return null;
        }

        while (currentPredator != null && Vector3.Distance(transform.position, currentPredator.transform.position) <= detectionRange) 
        {
            RunAwayFromPredator();

            yield return null;
        }

        if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        SetState(AnimalState.Idle);
    }

    private void RunAwayFromPredator()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)  
        {
                Vector3 runDir = transform.position - currentPredator.transform.position;
                Vector3 escapeDestination = transform.position + runDir.normalized * (escapeMaxDistance * 2);
                navMeshAgent.SetDestination(GetRandomNavMeshPosition(escapeDestination, escapeMaxDistance));
        }
    }

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
