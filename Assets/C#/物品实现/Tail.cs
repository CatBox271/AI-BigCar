using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour, ION,IFlip
{
    float lt;
    public SpriteRenderer sp;
    public Collider2D _collider;
    public Rigidbody2D rb;
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

            Vector3 v3 = transform.localScale;
            v3.x = Mathf.Abs(v3.x) * (flip ? -1 : 1);
            transform.localScale = v3;
        }
        Vector3 WA = wing.transform.localEulerAngles;
        if (NumSet == 0)
        {
            WA.z = Mathf.LerpAngle(WA.z, 0, Time.deltaTime * 5f);
        }
        else
        {
            if (NumSet > 0)
            {
                WA.z = Mathf.LerpAngle(WA.z, 15, Time.deltaTime * 5f);
            }
            if (NumSet < 0)
            {
                WA.z = Mathf.LerpAngle(WA.z, -15, Time.deltaTime * 5f);
            }
        }
        wing.transform.localEulerAngles = WA;


    }
    public float times;

    public float UpDown;

    public GameObject wing;

    public bool OnSet { get; set; }
    public float NumSet { get; set; }

    private void FixedUpdate()
    {
        float Right = Vector3.Dot( transform.right , rb.velocity);
        float UP = Vector3.Dot( transform.up , rb.velocity);
        rb.AddForce(transform.right * Right * times);
        if (NumSet > 0)
        {
            rb.AddTorque(UP * UpDown * (flip ? -1 : 1));
        }
        if (NumSet < 0)
        {
            rb.AddTorque(UP * -UpDown * (flip ? -1 : 1));
        }
    }
}
