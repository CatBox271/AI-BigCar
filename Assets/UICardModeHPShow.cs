using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICardModeHPShow : MonoBehaviour
{
    public Txt main;
    public Txt other;
    public bool Left;
    private void LateUpdate()
    {
        main.context = ((int)(Left ? TowelHPRecord.LeftHeath : TowelHPRecord.RightHeath).x).ToString();
        other.context = ((int)(Left ? TowelHPRecord.LeftHeath : TowelHPRecord.RightHeath).y).ToString();
        if (Left) TowelHPRecord.LeftHeath = new();
        else TowelHPRecord.RightHeath = new();
    }
}
