using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthenInWater : MonoBehaviour
{
    bool InWater = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (InWater == true) return;
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave _))
            {
                InWater = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (InWater == false) return;
        if (collision.CompareTag("Ground"))
        {
            if (collision.TryGetComponent(out WaterWave _))
            {
                InWater = false;
            }
        }
    }

    public Propellers per;
    public float Times;
    // Update is called once per frame
    int Strengthened = 0;
    void FixedUpdate()
    {
        if (InWater)
        {
            if (Strengthened != 1)
            {
                per.Force *= Times;
                Strengthened = 1;
            }
        }
        else
        {
            if (Strengthened == 1)
            {
                per.Force /= Times;
                Strengthened = -1;
            }
        }
    }
}
