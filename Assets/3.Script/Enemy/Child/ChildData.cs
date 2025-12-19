using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Child", menuName = "Christmas/Child Data")]
public class ChildData : ScriptableObject
{
    [Header("아이 정보")]
    public string childName;
    [TextArea(3, 5)]
    public string description;
    public Sprite portrait;

    [Header("선물 정보")]
    public GiftType desiredGift;
}
