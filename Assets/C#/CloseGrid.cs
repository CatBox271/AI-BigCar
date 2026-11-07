using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseGrid : MonoBehaviour,IStart
{
    public Sprite CloseSprite;
    Sprite OpenSprite;
    public void SetStart(float t)
    {
        if (SceneProp.Instance == null || !SceneProp.Instance.VideoMode)
        {
            SpriteRenderer sp = GetComponent<SpriteRenderer>();
            OpenSprite = sp.sprite;
            sp.sprite = CloseSprite;
        }
    }
}
