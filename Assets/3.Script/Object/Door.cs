using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;

    private bool isOpen = false;
    private bool isBusy = false; // 애니메이션 중 잠금

    private static readonly int OpenHash = Animator.StringToHash("Open");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isBusy) return; 

        isOpen = !isOpen;
        animator.SetBool(OpenHash, isOpen);
        isBusy = true;
    }

    // Animation Event (Opening / Closing 끝 프레임)
    public void OnDoorAnimationEnd()
    {
        isBusy = false;
    }
}