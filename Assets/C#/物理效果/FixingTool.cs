using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class FixingTool : MonoBehaviour
{
    private Heath heath;
    public float fix_value;
    public float RestTime;
    private float TimeKeep;
    public float Radius;
    public GameObject FixTrail;

    public GameObject FixShow;
    void Start()
    {
        TryGetComponent(out heath);
    }
    void Update()
    {
        if (heath != null && heath.camp == 0) return;
        TimeKeep -= Time.deltaTime;
        if (TimeKeep < 0)
        {
            TimeKeep = RestTime;
            FindingFixing();
        }
    }

    public Heath AimH;
    private void FindingFixing()
    {
        if (heath != null)
        {
            //Finding
            Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position, Radius);
            AimH = null;
            for (int i = 0; i < collider2D.Length; i++)
            {
                if (!collider2D[i].TryGetComponent<Heath>(out var h)) continue;
                if (h.camp != heath.camp) continue;
                if (h.die) continue;
                if (h.HP == h.max) continue;
                if (AimH == null)
                {
                    AimH = h;
                }
                else
                {
                    if (h.HP < AimH.HP)
                    {
                        AimH = h;
                    }
                }
            }
            if (AimH != null)
            {
                if (AimH.HP < AimH.max)
                {
                    //Fixing
                    float va = fix_value;
                    if (AimH.max - AimH.HP > fix_value)
                    {
                        AimH.HP += fix_value;
                    }
                    else
                    {
                        va = AimH.max - AimH.HP;
                        AimH.HP = AimH.max;
                    }
                    Instantiate(FixShow, AimH.transform.position, transform.rotation, transform.parent).GetComponent<FixShow>().Setting(va);
                    Instantiate(FixTrail, transform.position, transform.rotation, transform.parent).GetComponent<FixingTrail>().FT = this;
                }
            }
        }
        else
        {
            //Finding
            Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position, Radius);
            AimH = null;
            for (int i = 0; i < collider2D.Length; i++)
            {
                if (!collider2D[i].TryGetComponent<Heath>(out var h)) continue;
                if (h.die) continue;
                if (h.HP == h.max) continue;
                if (AimH == null)
                {
                    AimH = h;
                }
                else
                {
                    if (h.HP < AimH.HP)
                    {
                        AimH = h;
                    }
                }
            }
            if (AimH != null)
            {
                if (AimH.HP < AimH.max)
                {
                    //Fixing
                    float va = fix_value;
                    if (AimH.max - AimH.HP > fix_value)
                    {
                        AimH.HP += fix_value;
                    }
                    else
                    {
                        va = AimH.max - AimH.HP;
                        AimH.HP = AimH.max;
                    }
                    Instantiate(FixShow, AimH.transform.position, transform.rotation, transform.parent).GetComponent<FixShow>().Setting(va);
                    Instantiate(FixTrail, transform.position, transform.rotation, transform.parent).GetComponent<FixingTrail>().FT = this;
                }
            }
        }
    }
}
