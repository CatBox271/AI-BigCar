using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manifolds : MonoBehaviour
{
    public float speed;
    public float width;
    public Rigidbody2D rb;
    float startX;
    bool right;

    private void Awake()
    {
        startX = transform.position.x;
    }
    void FixedUpdate()
    {
        if (right)
        {
            rb.MovePosition(transform.position + new Vector3(speed*Time.fixedDeltaTime, 0, 0));
            if (transform.position.x - startX > width)
            {
                right = false;
            }
        }
        else
        {
            rb.MovePosition(transform.position + new Vector3(-speed*Time.fixedDeltaTime, 0, 0));
            if (startX - transform.position.x > width)
            {
                right = true;
            }
        }
    }
}
