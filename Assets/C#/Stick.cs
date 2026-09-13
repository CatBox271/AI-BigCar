using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick : MonoBehaviour
{
    public Collider2D Circle;
    public Rigidbody2D rb;
    public float Force;
    public List<Collider2D> rb_list = new(); 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D cc = collision.gameObject.GetComponent<Collider2D>();
        if (!rb_list.Contains(cc))
        {
            rb_list.Add(cc);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Collider2D cc = collision.gameObject.GetComponent<Collider2D>();
        rb_list.Remove(cc);

    }

    private void FixedUpdate()
    {
        for (int i = 0; i < rb_list.Count; i++)
        {
            Vector2 p = rb_list[i].ClosestPoint(transform.position);
            rb_list[i].TryGetComponent(out Rigidbody2D rb2);
            AddForceTo(rb2, p);
        }
    }

    void AddForceTo(Rigidbody2D _rb, Vector2 point)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        Vector2 offset = point - pos;
        float f1 = offset.magnitude - 0.499f * transform.lossyScale.x;
        Vector2 v2 = Mathf.Min(f1 * 50f,1) * Force * offset.normalized;
        if (_rb != null)
        {
            rb.AddForce(v2);
            _rb.AddForce(-v2);
        }
        else
        {
            rb.AddForce(2 * v2);
        }
    }
}
