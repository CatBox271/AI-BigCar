using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundBreak : MonoBehaviour
{
    public int Times;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (TryGetComponent(out Heath h))
            {
                //h.enabled = true;
                h.Hit(h.max / Times, 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
