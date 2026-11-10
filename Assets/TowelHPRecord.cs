using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowelHPRecord : MonoBehaviour
{
    public bool Main = false;
    public static Vector2 LeftHeath;
    public static Vector2 RightHeath;

    private Heath h;
    private void Start()
    {
        if (!TryGetComponent(out h)) h = GetComponentInChildren<Heath>();
    }
    private void Update()
    {
        if (h.camp == 2)
        {
            if (Main)
            {
                LeftHeath.x += h.HP;
            }
            else
            {
                LeftHeath.y += h.HP;
            }
        }
        else if(h.camp == 1)
        {
            if (Main)
            {
                RightHeath.x += h.HP;
            }
            else
            {
                RightHeath.y += h.HP;
            }
        }
    }
}
