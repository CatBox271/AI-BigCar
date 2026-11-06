using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Frames : MonoBehaviour
{
    public Text txt;
    private float t;
    private int a = 0;
    public bool Low = false;
    void Update()
    {
        if (Low)
        {
            if (t < Time.deltaTime) t = Time.deltaTime;
            a++;
            if (a < 60) return;
            a = 0;
            txt.text = Mathf.Round(1 / t).ToString();
            t = 0;
        }
        else
        {
            t += Time.deltaTime;
            a++;
            if (a < 60) return;
            a = 0;
            txt.text = Mathf.Round(60f / t).ToString();
            t = 0;
        }
    }
}
