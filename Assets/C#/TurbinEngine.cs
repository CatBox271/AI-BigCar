using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurbinEngine : MonoBehaviour
{
    public float MaxForce;
    public float ForceAddingSpeed;
    public bool State;
    public bool flip;
    public Rigidbody2D rb;
    float RealForce = 0;
    private void FixedUpdate()
    {
        if (GameManager.gameState != GameManager.GameState.Start) return;
        if (State)
        {
            if (RealForce != MaxForce)
            {
                RealForce += ForceAddingSpeed * Time.fixedDeltaTime;
                if (RealForce > MaxForce) RealForce = MaxForce;
            }
            rb.AddForce(transform.up * -RealForce * (flip ? -0.5f : 1));
        }
        else
        {
            if (RealForce != 0)
            {
                RealForce -= ForceAddingSpeed * Time.fixedDeltaTime * 5f;
                if (RealForce < 0) RealForce = 0;
                rb.AddForce(transform.up * -RealForce * (flip ? -0.5f : 1));
            }
        }
    }
    private Vector3 lp;
    private void Start()
    {
        lp = transform.GetChild(0).transform.localPosition;
    }
    private void Update()
    {
        if (flip)
        {
            transform.GetChild(0).transform.localPosition = lp + new Vector3(0f, 0.144f);
        }
        else
        {
            transform.GetChild(0).transform.localPosition = lp;
        }
    }
}
