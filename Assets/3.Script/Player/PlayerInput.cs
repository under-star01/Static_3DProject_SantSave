using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour { 
    
    public Vector2 viewingValue = Vector2.zero; 
    private PlayerInputAction playerInput;
    private PlayerMove playerMove;
    private PlayerSkill playerSkill;

    private void Awake() 
    { 
        // 컴포넌트 연결
        TryGetComponent(out playerMove);
        TryGetComponent(out playerSkill);

        // InputAction 생성
        playerInput = new PlayerInputAction();
    } 
    
    private void OnEnable() 
    { 
        // InputAction 연결 및 활성화
        playerInput.Player.Move.performed += OnMove; 
        playerInput.Player.Move.canceled += OnMove;
        playerInput.Player.Look.performed += OnLook;

        playerInput.Player.TransformSkill.performed += OnTransformSkill;
        playerInput.Player.DecoySkillStart.performed += OnDecoySkillStart;
        playerInput.Player.DecoySkillThrow.performed += OnDecoySkillThrow;
        playerInput.Enable(); 
    } 
    
    private void OnDisable() 
    {
        // InputAction 해제 및 비활성화
        playerInput.Player.Move.performed -= OnMove; 
        playerInput.Player.Move.canceled -= OnMove;
        playerInput.Player.Look.performed -= OnLook;
        playerInput.Disable(); 
    }

    // WASD 방향키 이동 메소드
    public void OnMove(InputAction.CallbackContext context) 
    {
        playerMove.SetMoveInput(context.ReadValue<Vector2>());
    } 
    
    // 화면 회전 메소드
    private void OnLook(InputAction.CallbackContext context)
    {
        playerMove.SetLookInput(context.ReadValue<Vector2>());
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