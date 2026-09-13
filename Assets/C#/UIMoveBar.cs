using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveBar : MonoBehaviour
{
    public Vector2 RelativePos;
    Vector2 last;
    private void Awake()
    {
        last = RelativePos;
    }
    private void LateUpdate()
    {
        if (last != RelativePos)
        {
            Vector3 change = RelativePos - last;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.localPosition += change;
            }
            last = RelativePos;
        }
    }
}
