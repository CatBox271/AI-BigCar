using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing : MonoBehaviour,IFlip
{
    float lt;
    public SpriteRenderer sp;
    public Collider2D _collider;
    public bool flip { get; set; }

    private void Update()
    {
        if (GameManager.gameState == GameManager.GameState.Ready)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                {
                    lt = Time.time;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                {
                    if (Time.time - lt < 0.2f)
                    {
                        flip = !flip;
                    }
                }
            }
            sp.flipX = flip;
        }
    }
    public Rigidbody2D rb;

    public float UpTimes = 6;
    public float RestitenceTimes = 5;

    private void FixedUpdate()
    {
        float Right = Vector3.Dot(flip ? -transform.right : transform.right, rb.velocity);
        //ษýมฆ
        rb.AddForce(transform.up * Right * UpTimes);
        //ื่มฆ
        rb.AddForce(-transform.right * Right * RestitenceTimes * (flip ? -1 : 1));
    }
}
