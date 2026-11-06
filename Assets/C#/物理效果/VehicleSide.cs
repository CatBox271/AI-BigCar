using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSide : MonoBehaviour
{
    public int Camp;
    int t = 0;
    bool StartWork;
    private List<float> all = new();

    public bool Middle;
    public bool Right;
    private void FixedUpdate()
    {
        t++;
        if (t < 10) return;
        StartWork = true;
        if (t == 10)
        {
            //重置
            all = new();
        }
        if (t == 11)
        {
            StartWork = false;
            t = 0;
            //分析数据
            float Closest = 1000f;
            for (int i = 0; i < all.Count; i++)
            {
                if (Mathf.Abs(all[i]) < Mathf.Abs(Closest))
                {
                    Closest = all[i];
                }
            }
            if (Closest != 1000f)
            {
                Right = Closest > 0;
                Middle = false;
            }
            else
            {
                Middle = true;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!StartWork) return;
        if (collision.gameObject.TryGetComponent(out Heath h))
        {
            if (h.camp != Camp)
            {
                all.Add(collision.transform.position.x);
            }
        }
    }
}
