using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class Predator : Animal
{
    [Header("Predator")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float maxChaseTime = 10f;
    [SerializeField] private int biteDamage = 3;
    [SerializeField] private float biteCoolDown = 1f;
    [SerializeField] private float attackDistance = 3f;

    private Prey currentChaseTarget = null;

    protected override void CheckChaseConditions()
    {
        if (currentChaseTarget != null)
        {
            return;
        }

        Collider[] colliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders);

        for(int i = 0;i < numColliders; i++)
        {
            Prey prey = colliders[i].GetComponent<Prey>();
            if (prey != null)
            {
                StartChase(prey);
                return;
            }
        }
        currentChaseTarget = null;
    }

    private void StartChase(Prey prey)
    {
        currentChaseTarget = prey;
        SetState(AnimalState.Chase);
    }

    protected override void HandleChaseState()
    {
        if (currentChaseTarget != null)
        {
            currentChaseTarget.AlterPrey(this);
            StartCoroutine(ChasePrey());
        }
        else
        {
            SetState(AnimalState.Idle);
        }
    }

    private IEnumerator ChasePrey()
    {
        float startTime = Time.time;

        while (currentChaseTarget != null && Vector3.Distance(transform.position, currentChaseTarget.transform.position) > attackDistance) 
        {
            if (Time.time - startTime >= maxChaseTime || currentChaseTarget == null)
            {
                StopChase();
                yield break;
            }
            SetState(AnimalState.Chase);
            navMeshAgent.SetDestination(currentChaseTarget.transform.position);
            yield return null;
        }


        if (currentChaseTarget)
        {
            currentChaseTarget.RecieveDamage(biteDamage);
        }

        yield return new WaitForSeconds(biteCoolDown);//¹¥»÷ÀäÈ´
        currentChaseTarget = null;
        HandleChaseState();

        CheckChaseConditions();
    }

    private void StopChase()
    {
        navMeshAgent.ResetPath();
        currentChaseTarget = null;
        SetState(AnimalState.Idle);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
