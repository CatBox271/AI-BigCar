using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirResistance : MonoBehaviour
{
    public Rigidbody2D rb;
    private CheckAlive ca;
    private void Awake()
    {
        TryGetComponent(out ca);
        if (name.Contains("NoColliderBlock"))
        {
            AirEffect = 0.1f;
        }
    }
    float AirEffect = 1;


    public  float FS = 0.1f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave w))
            {
                FS += w.Resistance;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave w))
            {
                FS -= w.Resistance;
            }
        }
    }
    Vector3 v3;
    private void FixedUpdate()
    {
        if (ca != null)
        {
            if (ca.BlockOffset.Contains(-4)) return;

        }
        else
        { 
        
        }
    }
}
