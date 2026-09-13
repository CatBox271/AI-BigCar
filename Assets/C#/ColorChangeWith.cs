using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeWith : MonoBehaviour
{
    public SpriteRenderer _towel;
    private SpriteRenderer sp;
    public float light_value;

    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (sp == null || _towel == null)
        {
            Destroy(this);
            return;
        }
        Color c = _towel.color + light_value * Color.white;
        c.a = _towel.color.a;
        sp.color = c;
    }
}
