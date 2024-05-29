using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RotatorToGroundNormal : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform parent;

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position,Vector3.down,out hit))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed *Time.deltaTime);
        }
    }
}
