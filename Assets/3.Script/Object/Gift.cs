using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gift : MonoBehaviour, IInteractable
{
    public GiftType_SO currentGift;
    [SerializeField] private bool isPicked = false;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out col);
    }

    public void Interact(PlayerInteract playerInteract)
    {
        if (playerInteract == null || isPicked) return;

        playerInteract.TryPickUp_Gift(this);
    }

    // 픽업 상태에 따른 선물 충돌 상태 변경 메소드
    public void SetPicked(bool picked)
    {
        if (rb != null)
        {
            rb.isKinematic = picked;
            rb.useGravity = !picked;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            col.enabled = !picked;
        }
    }
}
