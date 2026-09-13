using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailColor : MonoBehaviour
{
    public TrailRenderer tr;
    public SpriteRenderer sp;
    void Update()
    {
        tr.startColor = sp.color * 0.75f;
        tr.endColor = sp.color * 0.25f;
    }
}
