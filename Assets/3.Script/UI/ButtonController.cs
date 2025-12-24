using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText; // label 유니티에서 연결
    [SerializeField] private Image backgroundImage; // 사진 넣을것

    public void Initialize(string label, UnityAction onClick)
    {
        if (labelText != null)
        {
            labelText.text = label; // 라벨에 맞춰 글씨 띄움
        }
        
        var btn = GetComponent<Button>(); // btn에게 버튼 역할 부여
        if (btn != null) // 버튼이 있으면
        {
            btn.onClick.RemoveAllListeners(); // 버튼 클릭 이벤트 발생 안하면 알람없음
            if (onClick != null) // 클릭 되면
            {
                btn.onClick.AddListener(onClick); // 구독자 알람 발생
            }
        }
    }

    
    public void SetHighlight(bool on) // 선택/마우스 오버 시 시각 효과
    {
        if (backgroundImage != null) // 배경 이미지가 있으면
        {
            backgroundImage.color = on ? Color.white : Color.gray; // 활성화시 흰색 비활성화시 회색
        }
    }
}
