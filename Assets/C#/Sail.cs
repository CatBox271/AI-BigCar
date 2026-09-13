using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sail : MonoBehaviour,ION
{
    public float value;
    public Transform flip;
    public Transform sail;
    public float aimY;
    public float aimH;
    public Rigidbody2D rb;

    public bool OnSet { get; set; } = false;
    public float NumSet { get; set; } = 0;

    bool LO;
    float LN;
    private void Awake()
    {
        Set(OnSet, NumSet);
    }
    public void Set(bool flip, float on)
    {
        NumSet = on;
        OnSet = flip;
        StopCoroutine(nameof(Anim));
        StartCoroutine(nameof(Anim));
    }

    private void FixedUpdate()
    {
        if (LO != OnSet || LN != NumSet)
        {
            LO = OnSet;
            LN = NumSet;
            Set(OnSet, NumSet);
        }
        if (NumSet == 0) return;
        float N = value * NumSet * (OnSet ? -1 : 1);
        float L = Mathf.Max(0, transform.up.y);
        rb.AddForce(Vector2.right * N * L);
    }

    IEnumerator Anim()
    {
        float t = 0;
        int F = OnSet ? -1:1;
        float H = aimH * NumSet;
        float Y = aimY + H / 2f;

        do
        {
            flip.localScale = new(Mathf.Lerp(flip.localScale.x, F, t), 1, 1);
            sail.localPosition = new(sail.localPosition.x, Mathf.Lerp(sail.localPosition.y, Y, t));
            sail.localScale = new(sail.localScale.x, Mathf.Lerp(sail.localScale.y, H, t), 1);
            t += Time.deltaTime;
            yield return null;
        } while (t < 1);
    }
}
