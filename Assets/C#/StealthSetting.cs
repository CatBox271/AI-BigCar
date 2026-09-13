using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthSetting : MonoBehaviour
{
    public Stealth father;
    Heath h;
    SpriteRenderer sp;
    Color orign;
    private void OnDestroy()
    {
        Back();
    }

    public void Back()
    {
        if(sp!= null)
        sp.color = orign;
    }
    private void Start()
    {
        TryGetComponent(out h);
        TryGetComponent(out sp);
        orign = sp.color;
        Color no = orign;
        no.a = 0.25f;
        sp.color = no;
        startHit = h.GetHitTimes;
    }
    int startHit;
    void FixedUpdate()
    {
        if (h.GetHitTimes != startHit)
        {
            father.SetValue(false);
        }
        if (father == null || !father.State)
        {
            Destroy(this);
        }
    }
}
