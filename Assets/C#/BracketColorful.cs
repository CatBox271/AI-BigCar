using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BracketColorful : MonoBehaviour
{
    public SwarmCode sc;
    public static int num;

    private void Awake()
    {
        if (!sc.Host)
        {
            GetComponent<Image>().color = Color.HSVToRGB(num / 12f % 1, 1, 1 - (num / 12) * 0.25f % 0.75f);
            num++;
            Destroy(this);
        }
    }
}
