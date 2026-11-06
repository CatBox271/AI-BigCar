using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFire : MonoBehaviour
{
    public float value;
    private Heath h;
    private Rigidbody2D rb;
    private void Awake()
    {
        TryGetComponent(out h);
        TryGetComponent(out rb);
    }
    int a = 0;
    int b = 0;
    private void FixedUpdate()
    {
        a++;
        if (a < 150) return;
        a = 0;
        if (value <= 0)
        {
            Destroy(gameObject);
            return;
        }
        h.Hit(value * 3f, 0,true);
        if (rb.velocity.magnitude < 5)
        {
            value -= 0.02f;
        }
        else
        {
            if (rb.velocity.magnitude > 15)
            {
                if (value > 0.5f)
                {
                    value += 0.05f;
                }
                else
                {
                    value -= 0.1f;
                }
            }
        }

        if (InWater) value -= 0.1f;

        b++;
        if (b < 2) return;
        b = 0;

        if(value>0.25f) FireSpread();
    }

    public void FireSpread()
    {
        // 中立 6/9/12、左 7/10/13、右 8/11/14 都参与蔓延，再剔掉自己阵营那三层
        int mask = 1 << 0
                 | 1 << 6 | 1 << 7 | 1 << 8
                 | 1 << 9 | 1 << 10 | 1 << 11
                 | 1 << 12 | 1 << 13 | 1 << 14;
        if (h.camp == 1) mask &= ~(1 << 8 | 1 << 11 | 1 << 14);
        else if (h.camp == 2) mask &= ~(1 << 7 | 1 << 10 | 1 << 13);
        else mask &= ~(1 << 6 | 1 << 9 | 1 << 12);
        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, 1f, mask);
        for (int i = 0; i < col.Length; i++)
        {
            if (Random.Range(0, 5) == 0)
            {
                if (col[i].TryGetComponent(out OnFire f))
                {
                    f.value += 0.1f;
                }
                else
                {
                    col[i].gameObject.AddComponent<OnFire>().value = 0.1f;
                }
            }
        }
    }

    bool InWater = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (InWater == true) return;
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave _))
            {
                InWater = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (InWater == false) return;
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave _))
            {
                InWater = false;
            }
        }
    }
}
