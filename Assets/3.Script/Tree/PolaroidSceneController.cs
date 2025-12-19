using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 폴라로이드 UI 전체 흐름을 제어하는 씬 컨트롤러
/// - "첫 트리 상호작용" 때 3장의 폴라로이드 세션을 생성하고 UI를 강제로 오픈
/// - 이후 TAB으로만 열고/닫기 가능 (첫 상호작용 전에는 TAB 무시)
/// - 열릴 때: 배경 Dimmer ON + Time.timeScale = 0 (시간 정지)
/// - 닫힐 때: Dimmer OFF + Time.timeScale 복구
/// - 좌/우 버튼 OnClick 및 키보드 좌/우 입력으로 카드 넘기기 가능
///
/// 의존 대상:
/// - PolaroidManager : 이번 판(세션)에 사용할 3장 카드 세션 생성/인덱스 이동
/// - PolaroidBinder  : 현재 선택된 카드 데이터를 UI에 바인딩
/// </summary>
public class PolaroidSceneController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private PolaroidManager polaroidManager;

    [Header("UI Root")]
    [SerializeField] private GameObject polaroidPanel;   // PolaroidPanel (전체 컨테이너)
    [SerializeField] private GameObject dimmer;          // Dimmer (검은 배경) - 선택이지만 분리해두면 제어가 편함
    [SerializeField] private PolaroidBinder binder;      // CardRoot에 붙은 바인더(사진/텍스트 UI 갱신)

    [Header("New Input System (InputActionReference)")]
    [SerializeField] private InputActionReference actionToggle; // TAB
    [SerializeField] private InputActionReference actionNext;   // RightArrow
    [SerializeField] private InputActionReference actionPrev;   // LeftArrow
    [SerializeField] private InputActionReference actionClose;  // ESC

    // 첫 트리 상호작용 이후에만 TAB으로 열 수 있게 하는 플래그
    // - false: 아직 폴라로이드 획득 전 (TAB 입력 무시)
    // - true : 획득 이후 (TAB 토글 가능)
    private bool isUnlocked = false;

    // 현재 패널이 열려있는지 상태
    private bool isOpen = false;

    // 시간정지 전 timescale 저장용
    // (다른 시스템이 timeScale을 바꾸는 게임이라면, 중앙 TimeManager로 분리하는 게 더 안전함)
    private float prevTimeScale = 1.0f;

    private void Start()
    {
        // 시작 시에는 무조건 닫힌 상태로 정리
        // (에디터에서 Panel을 켜둔 채로 저장했더라도, 런타임에서는 꺼진 상태로 강제)
        SetOpen(false);
    }

    private void OnEnable()
    {
        // InputActionReference는 씬에서 할당한 Input Actions의 특정 Action을 참조하는 방식
        // 여기서 Enable + performed 이벤트 구독을 해줘야 입력이 들어온다.

        if (actionToggle != null && actionToggle.action != null)
        {
            actionToggle.action.performed += OnTogglePerformed;
            actionToggle.action.Enable();
        }

        if (actionNext != null && actionNext.action != null)
        {
            actionNext.action.performed += OnNextPerformed;
            actionNext.action.Enable();
        }

        if (actionPrev != null && actionPrev.action != null)
        {
            actionPrev.action.performed += OnPrevPerformed;
            actionPrev.action.Enable();
        }

        if (actionClose != null && actionClose.action != null)
        {
            actionClose.action.performed += OnClosePerformed;
            actionClose.action.Enable();
        }
    }

    private void OnDisable()
    {
        // 구독 해제/Disable을 해두면 씬 전환/오브젝트 비활성화 때 입력 중복 구독 문제를 예방할 수 있음

        if (actionToggle != null && actionToggle.action != null)
        {
            actionToggle.action.performed -= OnTogglePerformed;
            actionToggle.action.Disable();
        }

        if (actionNext != null && actionNext.action != null)
        {
            actionNext.action.performed -= OnNextPerformed;
            actionNext.action.Disable();
        }

        if (actionPrev != null && actionPrev.action != null)
        {
            actionPrev.action.performed -= OnPrevPerformed;
            actionPrev.action.Disable();
        }

        if (actionClose != null && actionClose.action != null)
        {
            actionClose.action.performed -= OnClosePerformed;
            actionClose.action.Disable();
        }
    }

    // =========================
    // 외부(트리 상호작용)에서 호출할 핵심 진입점
    // =========================

    /// <summary>
    /// "첫 트리 상호작용" 순간에 호출하면 된다.
    /// - 최초 1회만 세션(3장) 생성
    /// - 이후에도 호출하면 강제로 UI를 열어주는 용도로도 사용 가능(원치 않으면 최초 1회만 호출하도록 트리에서 제어)
    /// </summary>
    public void OnFirstTreeInteracted_AcquirePolaroids()
    {
        if (polaroidManager == null)
        {
            Debug.LogError("[PolaroidSceneController] polaroidManager가 비어있습니다.");
            return;
        }

        // 아직 획득 전이면: 이번 판(런)에서 사용할 3장 세션 생성
        if (isUnlocked == false)
        {
            polaroidManager.CreateNewSession();
            isUnlocked = true;
        }

        // 첫 획득 연출 요구사항: 무조건 열고 + 시간정지
        OpenAndFreeze();
    }

    /// <summary>
    /// 엔딩 후 "재시작" 같은 새 런 시작 시 호출하는 용도
    /// - 새 조합으로 다시 생성
    /// - TAB 열람 가능 상태 유지
    /// </summary>
    public void NewRun_RegeneratePolaroids()
    {
        if (polaroidManager == null)
        {
            return;
        }

        polaroidManager.CreateNewSession();
        isUnlocked = true;

        // 재시작 직후 UI를 열지는 정책에 따라 다름.
        // 지금은 "데이터만 새로 만들고 UI는 그대로"를 기본으로 둠.
        // 필요하면 OpenAndFreeze()를 호출하도록 바꿔도 됨.
        if (isOpen == true)
        {
            RefreshUI();
        }
    }

    // =========================
    // UI Button OnClick 연결용 (Unity Button 컴포넌트에서 직접 호출)
    // =========================

    public void OnClickNext()
    {
        // 패널이 열려있을 때만 넘기기 허용
        if (isOpen == false)
        {
            return;
        }

        if (polaroidManager == null)
        {
            return;
        }

        polaroidManager.Next();
        RefreshUI();
    }

    public void OnClickPrev()
    {
        if (isOpen == false)
        {
            return;
        }

        if (polaroidManager == null)
        {
            return;
        }

        polaroidManager.Prev();
        RefreshUI();
    }

    public void OnClickClose()
    {
        if (isOpen == false)
        {
            return;
        }

        CloseAndUnfreeze();
    }

    // =========================
    // Input System 콜백들
    // - actionToggle(TAB) 등은 InputActionReference로 연결됨
    // =========================

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        // 획득 전에는 TAB 기능 자체를 잠금
        if (isUnlocked == false)
        {
            return;
        }

        if (isOpen == false)
        {
            OpenAndFreeze();
        }
        else
        {
            CloseAndUnfreeze();
        }
    }

    private void OnNextPerformed(InputAction.CallbackContext ctx)
    {
        // 열려있는 상태일 때만 키보드로 넘기기 허용
        if (isOpen == false)
        {
            return;
        }

        OnClickNext();
    }

    private void OnPrevPerformed(InputAction.CallbackContext ctx)
    {
        if (isOpen == false)
        {
            return;
        }

        OnClickPrev();
    }

    private void OnClosePerformed(InputAction.CallbackContext ctx)
    {
        if (isOpen == false)
        {
            return;
        }

        CloseAndUnfreeze();
    }

    // =========================
    // 내부 유틸: 열기/닫기 + 시간정지 + UI 갱신
    // =========================

    private void OpenAndFreeze()
    {
        SetOpen(true);
        FreezeTime(true);
        RefreshUI();
    }

    private void CloseAndUnfreeze()
    {
        SetOpen(false);
        FreezeTime(false);
    }

    /// <summary>
    /// 패널 표시 상태를 실제 GameObject 활성/비활성으로 반영
    /// </summary>
    private void SetOpen(bool open)
    {
        isOpen = open;

        // 패널 전체를 끄고 켜는 방식이 가장 단순하고 안정적
        if (polaroidPanel != null)
        {
            polaroidPanel.SetActive(open);
        }

        // Dimmer를 따로 분리해뒀다면 여기서 함께 제어 가능
        if (dimmer != null)
        {
            dimmer.SetActive(open);
        }

        // 닫힐 때는 UI를 비워두면, 다음에 열 때 "이전 프레임 잔상"이 줄어듦
        if (open == false)
        {
            if (binder != null)
            {
                binder.Clear();
            }
        }
    }

    /// <summary>
    /// 시간 정지/복구
    /// - freeze=true : 현재 timeScale 저장 후 0으로
    /// - freeze=false: 저장해둔 timeScale로 복구
    /// </summary>
    private void FreezeTime(bool freeze)
    {
        if (freeze)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = prevTimeScale;
        }
    }

    /// <summary>
    /// 현재 선택된 카드(PolaroidManager의 현재 인덱스)를 UI에 바인딩
    /// </summary>
    private void RefreshUI()
    {
        if (binder == null)
        {
            return;
        }

        if (polaroidManager == null)
        {
            binder.Clear();
            return;
        }

        WishCardDataSO card = polaroidManager.GetCurrentCard();
        binder.Bind(card);
    }
}
