using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invincible : MonoBehaviour
{
    public float time;
    public Heath h;
    float start_HP;
    private void OnValidate()
    {
        TryGetComponent(out h);
    }
    private void Awake()
    {
        start_HP = h.HP;
    }
    private void FixedUpdate()
    {
        time -= Time.fixedDeltaTime;
        if (time <= 0) Destroy(this);
        else
        {
            h.HP = start_HP;
        }
    }
}
