using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Umbrella : MonoBehaviour,ION
{
    public Animator anim;
    public Rigidbody2D rb;
    public float Times;
    public float Speed;
    public BoxCollider2D OpenCollider;
    public BoxCollider2D CloseCollider;

    public bool OnSet { get; set; } = true;
    public float NumSet { get; set; }

    void FixedUpdate()
    {
        float f = anim.GetFloat("Value");
        if (OnSet)
        {
            f = Mathf.Lerp(f, 0, Time.fixedDeltaTime * Speed);
        }
        else
        {
            f = Mathf.Lerp(f, 1, Time.fixedDeltaTime * Speed);
        }

        anim.SetFloat("Value", f);
        if (OnSet)
        {
            Vector2 HoritalSpeed = Vector2.Dot(rb.velocity, transform.up) * transform.up * Times;
            Vector2 VeritalSpeed = Vector2.Dot(rb.velocity, transform.right) * transform.right *0.1f* Times;
            rb.AddForceAtPosition(-HoritalSpeed, transform.up * 0.9f + transform.position);
            rb.AddForceAtPosition(-VeritalSpeed, transform.up * 0.9f + transform.position);
            if (HoritalSpeed.magnitude > 400)
            {
                GetComponent<Heath>().HP = 0;
            }
        }
        OpenCollider.enabled = OnSet;
        CloseCollider.enabled = !OnSet;
    }


}
