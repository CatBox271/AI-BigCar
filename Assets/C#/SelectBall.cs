using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBall : MonoBehaviour
{
    public SpriteRenderer sp;
    public Rigidbody2D rb;
    public bool Red;
    public static bool RedSending;
    public static bool BlueSending;
    void Start()
    {
        if (Red)
        {
            sp.color = Color.red;
            gameObject.layer = 15;   // 选球层（旧 RedSelectBall 的位置）
        }
        else
        {
            sp.color = new Color(0, 0.5f, 1);
            gameObject.layer = 16;   // 选球层（旧 BlueSelectBall 的位置）
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        rb.velocity = (rb.velocity.normalized + Random.insideUnitCircle * 1f).normalized * rb.velocity.magnitude;
    }

    private void FixedUpdate()
    {
        if (RedSending && Red) Destroy(gameObject);
        if (BlueSending && !Red) Destroy(gameObject);
    }
}
