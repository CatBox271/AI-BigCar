using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleButton : MonoBehaviour
{
    public Sprite _1;
    public Sprite _5;
    public Image im;
    public void Set(float Times)
    {
        if (Times == 1f)
        {
            im.sprite = _1;
        }
        else
        {
            im.sprite = _5;
        }
    }
}
