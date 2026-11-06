using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CIWSAnim : MonoBehaviour
{
    public List<SpriteRenderer> tfl;
    public float ro;
    public Artillery at;

    float lastCount;
    void Update()
    {
        if (at.count != lastCount)
        {
            ro = at.count * Mathf.PI * 2f / tfl.Count;
        }
        else
        {
            ro += Time.deltaTime * Mathf.PI * 2f / tfl.Count / at.coolTime;
        }
        lastCount = at.count;
        for (int i = 0; i < tfl.Count; i++)
        {
            float angle = ro + Mathf.PI * 2 / tfl.Count * i;
            tfl[i].transform.localPosition = new Vector3(Mathf.Cos(angle) *0.14f+1.07f, Mathf.Sin(angle) * 0.31f);
            if (Mathf.Cos(angle) > 0)
            {
                tfl[i].sortingOrder = 1;
            }
            else
            {
                tfl[i].sortingOrder = 3;
            }
        }
    }
}
