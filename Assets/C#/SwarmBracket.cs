using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwarmBracket : MonoBehaviour
{
    public SwarmCode sc;
    public GameObject Left;
    public GameObject Right;
    bool ready;
    private void Update()
    {
        if (!sc.Host)
        {
            if (sc.Fixed)
            {
                ready = true;
            }
            else
            {
                if (ready)
                {
                    Swarm();
                }
            }
        }
    }

    public SwarmCode[] Swarm(int to = -1)
    {
        GameObject go = Instantiate(Left, transform.parent);//生成左括弧
        SwarmCode gsc = go.GetComponent<SwarmCode>();
        go.transform.position = transform.position;
        gsc.TryFixOn();

        
        GameObject go1 = Instantiate(Right, transform.parent);//生成右括弧
        SwarmCode gsc1 = go1.GetComponent<SwarmCode>();
        if (to != -1)
        {
            gsc1.Fixed = true;
            gsc1.MoveTo(to);
        }
        gsc.GetComponent<DieWith>().dieWith = go1;
        gsc1.GetComponent<DieWith>().dieWith = go;
        Color c = GetComponent<Image>().color;
        go.GetComponent<Image>().color = c;
        go1.GetComponent<Image>().color = c;
        Destroy(gameObject);

        return new SwarmCode[] { gsc, gsc1 };
    }
}
