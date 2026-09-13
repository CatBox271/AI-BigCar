using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInform : MonoBehaviour
{
    public LevelKind lk;
    public void Off()
    {
        gameObject.SetActive(false);
    }
    public void GO()
    {
        if (lk != null)
        {
            lk.BeSelect();
        }
    }
}
