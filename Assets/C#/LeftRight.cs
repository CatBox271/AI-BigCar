using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftRight : MonoBehaviour
{
    public float LeftLim;
    public float RightLim;
    public float speed;
    private Rigidbody2D rb;
    public bool Towards;//false orignal
    private void Awake()
    {
        if (TryGetComponent(out rb))
        {
            rb.isKinematic = true;
        }
    }
    void Update()
    {
        if (rb == null)
        {
            transform.position += new Vector3(speed * Time.deltaTime, 0) * (Towards ? -1 : 1);
        }
        if (speed > 0)
        {
            if (Towards)
            {
                if (transform.position.x < LeftLim)
                {
                    Towards = false;
                }
            }
            else
            {
                if (transform.position.x > RightLim)
                {
                    Towards = true;
                }
            }
        }
        else
        {
            if (Towards)
            {
                if (transform.position.x > RightLim)
                {
                    Towards = false;
                }
            }
            else
            {
                if (transform.position.x < LeftLim)
                {
                    Towards = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(new Vector2(transform.position.x + speed * (Towards ? -1 : 1) * Time.fixedDeltaTime, transform.position.y));
        }
    }
}
