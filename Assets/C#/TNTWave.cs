using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNTWave : MonoBehaviour
{
    public int camp;
    private SpriteRenderer sp;
    private float radio;
    private float speed;
    private Color start_color;
    private float hit;
    public void Setting(Color _color, int _layer, float _radio, float _speed,float _hit,int _camp)
    {
        TryGetComponent(out sp);
        sp.color = _color;
        gameObject.layer = _layer;
        radio = _radio;
        speed = _speed;
        start_color = _color;
        hit = _hit;
        camp = _camp;
        CameraShake.start = Time.time;
    }
    void Update()
    {
        if (GameManager.gameState == GameManager.GameState.Ready)
        {
            Destroy(gameObject);
            return;
        }
        if (transform.localScale.x > radio)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.localScale += speed * Time.deltaTime * Vector3.one;
            Color gh = (radio - transform.localScale.x) / radio * start_color * 2f;
            gh.a = Mathf.Min(0.25f, gh.a);
            sp.color = gh;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        float value = hit * (radio / transform.localScale.x);
        if (collision.TryGetComponent(out Heath h))
        {
            if (h.camp == camp)
            {
                h.enabled = true;
                h.Hit(value / 20f, camp);
            }
            else
            {
                h.enabled = true;
                h.Hit(value, camp);
            }
        }
        if (collision.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            Vector2 n = (rb.transform.position - transform.position).normalized * value;
            rb.velocity += n / Mathf.Pow(Mathf.Max(rb.mass, 1), 2);
        }
    }
}
