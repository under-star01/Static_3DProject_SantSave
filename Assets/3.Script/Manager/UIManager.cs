using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    [SerializeField] private TextMeshProUGUI scoreUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetScore(int score)
    {
        scoreUI.text = $"Score : {score}";
    }
}
