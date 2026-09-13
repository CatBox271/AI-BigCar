using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishCharacter : MonoBehaviour
{
    public void Show(string Character,bool win)
    {
        print(Character);
        Image im = transform.GetChild(1).GetComponent<Image>();
        im.sprite = Resources.Load(Character + (win ? "Win" : "Lose")) as Sprite;
    }
}
