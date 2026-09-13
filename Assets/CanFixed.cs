using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanFixed : MonoBehaviour
{
    private Heath h;
    private Collider2D col;
    public float HP;
    public Artillery at;

    void Start()
    {
        TryGetComponent(out h);
        TryGetComponent(out col);
        h.ND();
        HP = h.HP;
    }

    bool Break;

    private void Update()
    {
        if (h.HP <= 0)
        {
            if (!Break)
            {
                gameObject.layer = 0;
                h.camp = 0;
                at.enabled = true;
                HP *= 1.25f;
                at.Hit *= 1.5f;
                Break = true;
                at.enabled = false;
            }
            col.isTrigger = false;
            if (h.HP < -HP)
            {
                h.HP = HP;
                h.max = HP;
                if (h.KeepRecord[1] > h.KeepRecord[2])
                {
                    h.Set(1);
                    gameObject.layer = 6;
                }
                else
                {
                    h.Set(2);
                    gameObject.layer = 7;
                }
                h.KeepRecord = new() { 0, 0, 0 };
                h.ND();
                at.camp = h.camp;
            }
        }
        else
        {
            if (h.camp == 0) return;
            if (Break)
            {
                Break = false;
            }
            at.enabled = true;
            gameObject.layer = h.camp == 1 ? 6 : 7;
        }
    }
}
