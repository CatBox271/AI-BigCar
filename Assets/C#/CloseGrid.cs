using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseGrid : MonoBehaviour,IStart
{
    public Sprite CloseSprite;
    Sprite OpenSprite;
    public void SetStart(float t)
    {
        SceneProp sceneProp = SceneProp.Instance;
        if (sceneProp == null || !(sceneProp.gameMode == SceneProp.GameMode.video || sceneProp.gameMode == SceneProp.GameMode.card))
        {
            SpriteRenderer sp = GetComponent<SpriteRenderer>();
            OpenSprite = sp.sprite;
            sp.sprite = CloseSprite;
        }
    }
}
