using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainsawHit : MonoBehaviour
{
    public float hit_value;
    private Heath h;
    public Transform Left;
    public Transform Right;
    public Transform UP;
    private void Awake()
    {
        TryGetComponent(out h);
    }
    void Update()
    {
        Anim();
    }
    public float anim_speed;
    private void Anim()
    {
        Left.localPosition = new Vector3(0.475f, 0.3441485f + Mathf.Sin(Time.time * anim_speed) * 0.125f);
        Right.localPosition = new Vector3(-0.475f, 0.3534258f - Mathf.Sin(Time.time * anim_speed) * 0.125f);
        UP.localEulerAngles = new(0, 0, Mathf.Sin(Time.time * anim_speed) * 16f);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!Hitable) return;
        if (h.camp == 0) return;
        if (collision.gameObject.TryGetComponent(out Heath heath))
        {
            if (heath.camp != h.camp)
            {
                heath.enabled = true;
                heath.Hit(hit_value * 30f * 0.02f, h.camp);
            }
        }
    }
    int a;
    private bool Hitable;
    private void FixedUpdate()
    {
        Hitable = false;
        a++;
        if (a < 2f) return;
        a = 0;
        Hitable = true;
    }
}
