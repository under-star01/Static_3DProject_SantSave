using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour
{

    public Vector2 viewingValue = Vector2.zero;
    private PlayerInputAction playerInput;
    private PlayerMove playerMove;
    private PlayerSkill playerSkill;
    private PlayerInteract playerInteract;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out playerMove);
        TryGetComponent(out playerSkill);
        TryGetComponent(out playerInteract);

        // InputAction 생성
        playerInput = new PlayerInputAction();
    }

    private void OnEnable()
    {
        // InputAction 연결 및 활성화
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;
        playerInput.Player.Look.performed += OnLook;
        playerInput.Player.Zoom.performed += OnSetZoom;

        playerInput.Player.TransformSkill.performed += OnTransformSkill;
        playerInput.Player.DecoySkillStart.performed += OnDecoySkillStart;
        playerInput.Player.DecoySkillThrow.performed += OnDecoySkillThrow;
        playerInput.Player.Run.performed += OnRunStart;
        playerInput.Player.Run.canceled += OnRunStop;
        playerInput.Player.Pick.performed += OnInteract;
        playerInput.Player.Drop.performed += OnDropGift;

        playerInput.Enable();
    }

    private void OnDisable()
    {
        // InputAction 해제 및 비활성화
        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnMove;
        playerInput.Player.Look.performed -= OnLook;
        playerInput.Player.Zoom.performed -= OnSetZoom;

        playerInput.Player.DecoySkillStart.performed -= OnDecoySkillStart;
        playerInput.Player.DecoySkillThrow.performed -= OnDecoySkillThrow;
        playerInput.Player.Run.performed -= OnRunStart;
        playerInput.Player.Run.canceled -= OnRunStop;
        playerInput.Player.Drop.performed -= OnDropGift;

        playerInput.Disable();
    }

    // WASD 방향키 이동 메소드
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 raw = context.ReadValue<Vector2>();

        float dead = 0.1f;
        float dirX = 0f;
        float dirY = 0f;

        // 1, 0, -1로 input 정리
        if (raw.x > dead) dirX = 1f;
        if (raw.x < -dead) dirX = -1f;
        if (raw.y > dead) dirY = 1f;
        if (raw.y < -dead) dirY = -1f;

        Vector2 move = new Vector2(dirX, dirY);
        playerMove.SetMoveInput(move);
    }

    // 화면 회전 메소드
    private void OnLook(InputAction.CallbackContext context)
    {
        playerMove.SetLookInput(context.ReadValue<Vector2>());
    }

    private void OnTransformSkill(InputAction.CallbackContext context)
    {
        if (playerSkill != null && playerInteract != null)
        {
            // 선물을 들고 있지 않을 때, 스킬 사용 가능
            if (!playerInteract.hasGift)
            {
                playerSkill.OnTransformSkill();
            }
        }
    }

    private void OnDecoySkillStart(InputAction.CallbackContext context)
    {
        if (playerSkill != null && playerInteract != null)
        {
            // 선물을 들고 있지 않을 때, 스킬 사용 가능
            if (!playerInteract.hasGift)
            {
                playerSkill.OnDecoySkillStart();
            }
        }
    }

    private void OnDecoySkillThrow(InputAction.CallbackContext context)
    {
        if (playerSkill != null && playerInteract != null)
        {
            // 선물을 들고 있지 않을 때, 스킬 사용 가능
            if (!playerInteract.hasGift)
            {
                playerSkill.OnDecoySkillThrow();
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInteract != null)
        {
            playerInteract.TryInteract();
        }
    }

    private void OnDropGift(InputAction.CallbackContext context)
    {
        if (playerInteract != null)
        {
            playerInteract.DropCarriedGift();
        }
    }

    private void OnRunStart(InputAction.CallbackContext context)
    {
        if (playerMove != null)
        {
            playerMove.RunStart();
        }
    }

    private void OnRunStop(InputAction.CallbackContext context)
    {
        if (playerMove != null)
        {
            playerMove.RunStop();
        }
    }

    private void OnSetZoom(InputAction.CallbackContext context)
    {
        if (playerMove != null)
        {
            playerMove.SetCameraZoom(context.ReadValue<float>());
        }
    }
}