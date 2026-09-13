using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieWith : MonoBehaviour
{
    public GameObject dieWith;
    private void Update()
    {
        if (dieWith == null)
        {
            Destroy(gameObject);
        }
        else
        {
            if (dieWith.TryGetComponent(out Heath h) && h.die)
            {
                Destroy(gameObject);
            }
        }
    }
}
