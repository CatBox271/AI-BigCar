using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Rigidbody2D rb) && !collision.name.Contains("laser"))
        {
            if (!rb.isKinematic)
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
