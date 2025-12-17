using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("쿨타임")]
    [SerializeField] private float transformCool = 0.5f; // 스킬 쿨타임

    [Header("이펙트")]
    [SerializeField] private ParticleSystem transformEffect;

    private Animator animator;
    private bool isTransformed = false; // 변신 상태
    private float lastSkillTime = -999f; // 마지막 스킬 사용 시간

    private void Awake()
    {
        TryGetComponent(out animator);
    }

    // Q키 입력 처리 (PlayerInput에서 호출)
    public void OnTransformSkill()
    {
        // 쿨타임 체크
        if (Time.time < lastSkillTime + transformCool)
        {
            return;
        }

        if (transformEffect != null)
        {
            transformEffect.Stop();
            transformEffect.Play();
        }

        // 애니메이션 트리거
        if (animator != null)
        {
            if (isTransformed)
            {
                animator.SetTrigger("Detransform");
                isTransformed = false;
            }
            else
            {
                animator.SetTrigger("Transform");
                isTransformed = true;
            }
        }

        lastSkillTime = Time.time;
    }
}
