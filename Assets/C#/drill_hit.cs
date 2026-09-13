using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class drill_hit : MonoBehaviour
{
    public float hit_value;
    public float CrushScale = 0.05f;
    private Heath selfHP;

    private int a;
    private void Awake()
    {
        selfHP = GetComponent<Heath>();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!Hitable) return;
        if (selfHP.camp == 0) return;
        if (collision.gameObject.TryGetComponent(out Heath heath))
        {
            if (heath.camp != selfHP.camp)
            {
                heath.enabled = true;
                heath.Hit(hit_value * 0.02f * 30f,selfHP.camp);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Heath heath))
        {
            if (collision.gameObject.TryGetComponent(out Rigidbody2D rb))
            {
                if (heath.camp != selfHP.camp)
                {
                    heath.enabled = true;
                    heath.Hit(hit_value * CrushScale * ((transform.GetComponent<Rigidbody2D>().velocity - rb.velocity) * 5f).sqrMagnitude, selfHP.camp);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        Hitable = false;
        a++;
        if (a < 7) return;
        a = 0;
        Hitable = true;
    }

    private bool Hitable;
}
