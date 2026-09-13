using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VanishSlowly : MonoBehaviour
{
    public Image im;
    public  bool On;
    float time = 1;
    void Update()
    {
        if (!On)
        {
            time-= Time.deltaTime / 1.5f;
            if (time < 0)
            {
                time = 0;
            }
            Color color = im.color;
            color.a = Mathf.Pow( time,0.5f);
            im.color = color;
        }
        else
        {
            time += Time.deltaTime / 1.5f;
            if (time > 1)
            {
                time = 1;
            }
            Color color = im.color;
            color.a = Mathf.Pow(time, 0.5f);
            im.color = color;
        }
    }
}
