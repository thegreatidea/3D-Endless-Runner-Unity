using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private PhysicsMaterial ragdollMaterial;

    void Start()
    {
        SetRigidbodyState(true);
        SetColliderState(false);
        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = true;
    }

    public void Die()
    {
        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = false;
        SetRigidbodyState(false);
        SetColliderState(true);
    }

    public void Die(Vector3 forceDirection, float forceAmount = 300f)
    {
        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = false;
        SetRigidbodyState(false);
        SetColliderState(true);

        // Apply force to all rigidbodies for a more dynamic ragdoll
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.AddForce(forceDirection * forceAmount, ForceMode.Impulse);
        }
    }

    void SetRigidbodyState(bool state)
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = state;
        }
        if (TryGetComponent<Rigidbody>(out var rootRb))
        {
            rootRb.isKinematic = !state;
        }
    }

    void SetColliderState(bool state)
    {
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = state;
            if (ragdollMaterial != null) col.material = ragdollMaterial;
        }
        if (TryGetComponent<Collider>(out var rootCol))
        {
            rootCol.enabled = !state;
            if (ragdollMaterial != null) rootCol.material = ragdollMaterial;
        }
    }
}