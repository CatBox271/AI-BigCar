using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RangeHit : MonoBehaviour
{
    public int camp;
    public float Hit;
    public float CD;
    float T = -1;
    List<Heath> HL = new();
    private void FixedUpdate()
    {
        T -= Time.fixedDeltaTime;
        if (T < 0)
        {
            T = CD;
            for (int i = HL.Count - 1; i > -1; i--)
            {
                if (HL[i] == null)
                {
                    HL.RemoveAt(i);
                    continue;
                }
                HL[i].enabled = true;
                HL[i].Hit(HL[i].max * Hit, camp,true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Heath h))
        {
            if (h.camp != camp)
            {
                HL.Add(h);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Heath h))
        {
            HL.Remove(h);
        }
    }
}
