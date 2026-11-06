using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Center : MonoBehaviour,IStart
{
    public CheckAlive ca;

    private ButtonManager bm;

    bool start;

    public bool out_die;
    public void SetStart(float t)
    {
        try
        {
            start = true;

            bm = GameObject.Find("Canvas").transform.Find("ButtonManager").GetComponent<ButtonManager>();//准备生成button
            if (ca.BlockOffset.Contains(-4)) SwarmButton();
        }
        catch
        { 
        
        }
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

    private void FixedUpdate()
    {
        if (!start) return;
        if (out_die)
        {
            if (ca.allConnect.Count == 0 || ca.allConnect[0] == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
