using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour
{
    private PlayerInputAction playerInput;
    private PlayerMove playerMove;
    private PlayerSkill playerSkill;

    private void Awake()
    {
        TryGetComponent(out playerMove);
        TryGetComponent(out playerSkill);

        playerInput = new PlayerInputAction();
    }

    private void OnEnable()
    {
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;

        // 변신 스킬 (Q)
        playerInput.Player.TransformSkill.performed += OnTransformSkill;

        // 디코이 스킬 (우클릭)
        playerInput.Player.DecoySkillStart.performed += OnDecoySkillStart;

        // 디코이 투척 (좌클릭)
        playerInput.Player.DecoySkillThrow.performed += OnDecoySkillThrow;

        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnMove;
        playerInput.Player.TransformSkill.performed -= OnTransformSkill;
        playerInput.Player.DecoySkillStart.performed -= OnDecoySkillStart;
        playerInput.Player.DecoySkillThrow.performed -= OnDecoySkillThrow;

        playerInput.Disable();
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            playerMove.SetLookInput(Mouse.current.position.ReadValue());
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        playerMove.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnTransformSkill(InputAction.CallbackContext context)
    {
        if (playerSkill != null)
        {
            playerSkill.OnTransformSkill();
        }
    }

    private void OnDecoySkillStart(InputAction.CallbackContext context)
    {
        if (playerSkill != null)
        {
            playerSkill.OnDecoySkillStart();
        }
    }

    private void OnDecoySkillThrow(InputAction.CallbackContext context)
    {
        if (playerSkill != null)
        {
            playerSkill.OnDecoySkillThrow();
        }
    }
}