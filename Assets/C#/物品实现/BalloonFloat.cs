using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonFloat : MonoBehaviour
{
    public Rigidbody2D rb;
    public float UpForce;
    public Balloon ball;
    private void FixedUpdate()
    {
        rb.AddForce(Vector2.up * UpForce * ball.value);
    }
}
