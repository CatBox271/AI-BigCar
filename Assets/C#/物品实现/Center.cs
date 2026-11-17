using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Center : MonoBehaviour,IStart
{
    public CheckAlive ca;
    public bool out_die;

    private ButtonManager bm;

    public static ClockBucket step = Clock.Bucket(1.7f);
    public void SetStart(float t)
    {
        out_die = SceneProp.Instance?.center_out_die ?? false;
        if ((bm = ButtonManager.Instance) != null && ca.BlockOffset.Contains(-4)) SwarmButton();
        step.OnDue += WhenStart;
    }

    public void SwarmButton()
    {
        bm.ClearButton();
        var BL = GameManager.allInfo[ca.VID];
        for (int i = 0; i < BL.Count; i++)
        {
            bm.AddButton(BL[i],ca);
        }
    }

    void WhenStart(float a)
    {
        if (out_die)
        {
            if (ca.allConnect.Count == 0 || ca.allConnect[0] == null)
            {
                Destroy(gameObject);
            }
        }
    }
    private void OnDestroy()
    {
        step.OnDue -= WhenStart;
    }
}
