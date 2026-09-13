using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour,ION
{
    public GameObject Ball;
    public Heath H;
    public Rigidbody2D RB;
    public TryConnect tc;
    public float Speed;
    public float Angle;
    float AngleOffset;
    int way;

    public bool OnSet { get; set; }
    public float NumSet { get; set; }

    private void Update()
    {
        if (OnSet)
        {
            OnSet = false;
            Shoot();
        }
    }

    public void Shoot()
    {
        Heath h = Instantiate(Ball, transform.position,transform.rotation).GetComponent<Heath>();
        Rigidbody2D rb = h.GetComponent<Rigidbody2D>();
        bullet bu = h.GetComponent<bullet>();
        h.camp = H.camp;
        rb.velocity = new Vector3(RB.velocity.x, RB.velocity.y) + Speed * 2 * (Quaternion.AngleAxis(Angle, Vector3.forward) * transform.right);
        RB.velocity = new Vector3(RB.velocity.x, RB.velocity.y) - Speed * 0.75f * rb.mass / RB.mass * (Quaternion.AngleAxis(Angle, Vector3.forward) * transform.right);
        rb.gameObject.layer = h.camp == 1 ? 8 : 9;
        bu._camp = h.camp;
        Destroy(h.gameObject, 30f);
    }

    private void LateUpdate()
    {
        if (GameManager.gameState != GameManager.GameState.Start)
        {
            for (int i = 0; i < 5; i++)
            {
                if (tc.CantConnect[i])
                {
                    way = i;
                    break;
                }
            }
            if (way != 4)
            {
                AngleOffset = way switch
                {
                    0 => 180,
                    1 => 0,
                    2 => -90,
                    3 => 90,
                };
            }
        }
        transform.GetChild(0).localEulerAngles = new Vector3(0,0, AngleOffset);
        transform.GetChild(1).localEulerAngles = new Vector3(0, 0, AngleOffset);
        transform.GetChild(2).localEulerAngles = new Vector3(0,0, Angle);
    }
}
