using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColor : MonoBehaviour
{
    public bool On;
    // Start is called before the first frame update
    void Start()
    {
        if (!On) return;
        Color c = Random.ColorHSV();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i). TryGetComponent(out SpriteRenderer sp))
            {
                sp.color = c;
            }
        }
    }
}
