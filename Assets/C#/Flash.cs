using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    SpriteRenderer sp;
    Color orign;
    private void Awake()
    {
        if (
        TryGetComponent(out sp))
        {
            orign = sp.color;
        }
    }
    private void Update()
    {
        Color g = orign;
        g.a = Mathf.Sin(Time.time * 2f) + 1.5f;
        g.a /= 3f;
        sp.color = g;
    }

}
