using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUmbrella : MonoBehaviour,IPower
{
    public Umbrella um;
    public bool On;
    public float Speed;
    public float Force;
    public Rigidbody2D rb;
    float TimeLast = 0;
    private void Update()
    {
        um.Speed = Speed;
        TimeLast -= Time.deltaTime * Speed;
        if (TimeLast <= 0)
        {
            if (On)
            {
                um.OnSet = !um.OnSet;
                TimeLast = 2f;
            }
        }
    }
    bool last = true;
    float TimeWork = 0;
    private void FixedUpdate()
    {
        if (um.OnSet != last)
        {
            last = um.OnSet;
            TimeWork = 0.02f;
        }
        else
        {
            if (TimeWork >= 0)
            {
                TimeWork -= Time.fixedDeltaTime;
                if (um.OnSet)
                {
                    rb.AddForceAtPosition(transform.up *Speed * Force / -2f, transform.position + transform.up * 0.9f);
                }
                else
                {
                    rb.AddForceAtPosition(transform.up * Speed * Force, transform.position + transform.up * 0.9f);
                }
            }
        }
    }

    public bool IsOn()
    {
        return On;
    }
    public float 发动机乘数 = 5;
    public void AddPower(float set)
    {
        Speed += set * 发动机乘数;
    }
}
