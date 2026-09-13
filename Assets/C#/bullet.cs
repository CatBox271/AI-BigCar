using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public bool Fire;
    public bool Laser;
    public int _camp;
    public float speed;
    public float hit_value;
    public bool die;
    public float Gravity;
    public Rigidbody2D rb;

    private SpriteRenderer sp;
    private float maxDistance = -2f;

    public TrailRenderer trail;

    public bool Cannon;

    public bool NotDie;
    public void SetInform(int Icamp, float Ispeed,float Ihit_value,Color Icolor)
    {
        _camp = Icamp;
        speed = Ispeed;
        hit_value = Ihit_value;

        GetComponent<SpriteRenderer>().color = Icolor;
        if (trail == null)
        {
            if (_camp == 1)
            {
                trail.startColor = new Color(0.25f, 0.25f, 1f);
                trail.endColor = new Color(0.25f, 0.25f, 1f);
            }
            if (_camp == 2)
            {
                trail.startColor = new Color(1, 0.25f, 0.25f);
                trail.endColor = new Color(1, 0.25f, 0.25f);

            }
            if (_camp == 0)
            {
                trail.startColor = new Color(1, 1f, 1f);
                trail.endColor = new Color(1, 1f, 1f);
            }
        }
        if (Laser)
        {
            StartCoroutine(ShootLaser());
        }
        else
        {
            Vector2 v2 = transform.right * Ispeed;
            rb.velocity += v2;
            rb.gravityScale = Gravity / 9.81f;
        }
    }
    public void SetDistance(float MD)
    {
        maxDistance = MD;
    }

    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();

    }
    private void Start()
    {
        if (!NotDie)
            if (maxDistance == -2)
            {
                Destroy(gameObject, 60f);
            }
            else
            {
                Destroy(gameObject, maxDistance / speed * 1.1f);
            }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Laser) return;
        if (collision.gameObject.CompareTag("EnergyShield"))
        {
            if (collision.transform.parent.TryGetComponent(out Shield shield))
            {
                shield.GetHit(hit_value,transform.position);
            }
            if (!die)
            {
                die = true;
                GetComponent<CircleCollider2D>().isTrigger = true;
                rb.velocity = new Vector2();
                if(!NotDie)
                Destroy(gameObject, 0.25f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.TryGetComponent(out Heath h))
        {
            if (!NotDie)
            {
                rb.velocity = new Vector2();
                Destroy(gameObject, 0.2f);
            }
            return;
        }
        if (Cannon)
        {
            if (!die)
            {
                if (h.camp != _camp)
                {
                    h.enabled = true;
                    h.Hit(hit_value, _camp);
                }
                die = true;
                if (!NotDie)
                    Destroy(gameObject, 0.25f);

            }
        }
        else
        {
            if (!die)
            {
                if (h.camp != _camp)
                {
                    h.enabled = true;
                    h.Hit(hit_value, _camp);
                }
                die = true;
                if (!NotDie)
                {
                    rb.velocity = new Vector2();
                    Destroy(gameObject, 0.25f);
                }
            }
        }
    }
    Heath h;
    private void Update()
    {
        if (Cannon)
        {
            if (h == null) TryGetComponent(out h);
            if (!die)
            {
                if (h.HP <= 0)
                {
                    die = true;
                    if (!NotDie)
                        Destroy(gameObject, 0.25f);
                }
            }
            else
            {
                foreach (Collider2D cd in GetComponents<Collider2D>())
                {
                    cd.isTrigger = false;
                }
                rb.velocity = new();
                sp.color -= new Color(0, 0, 0, 4f) * Time.deltaTime;
                transform.localScale += 4f * Time.deltaTime * Vector3.one;
            }
        }
        else
        {
            if (Laser)
            {

            }
            else
            {
                if (die)
                {

                    sp.color -= new Color(0, 0, 0, 4f) * Time.deltaTime;
                    transform.localScale += 4f * Time.deltaTime * Vector3.one;
                }
            }
        }

    }
    public bool OverCross;
    IEnumerator ShootLaser()
    {
        yield return null;
        List<RaycastHit2D> ray = new();
        ray.AddRange(Physics2D.LinecastAll(transform.position + transform.up * 0.75f, transform.position + transform.right * maxDistance + transform.up * 0.2f));
        ray.AddRange(Physics2D.LinecastAll(transform.position + transform.up * -0.75f, transform.position + transform.right * maxDistance + transform.up * -0.2f));
        List<RaycastHit2D> order = new();
        for (int i = 0; i < ray.Count; i++)
        {
            if ((ray[i].transform.CompareTag("EnergyShield") && ray[i].transform.parent.GetComponent<Heath>().camp != _camp || ray[i].transform.TryGetComponent(out Heath h) && h.camp != _camp) && !order.Exists(s=>s.transform == ray[i].transform))
            {
                bool Add = false;
                for (int or = 0; or < order.Count; or++)
                {
                    if (order[or].distance > ray[i].distance)
                    {
                        order.Insert(or, ray[i]);
                        Add = true;
                        break;
                    }
                }
                if (!Add)
                {
                    order.Add(ray[i]);
                }
            }
        }
        int last = -1;
        for (int i = 0; i < order.Count; i++)
        {
            last = i;
            if (order[i].transform.CompareTag("EnergyShield"))
            {
                if (order[i].transform.parent.TryGetComponent(out Shield shield))
                {
                    shield.GetHit((1.5f - order[i].distance / maxDistance) * 8f * hit_value, order[i].point);
                }
                transform.position = order[i].point;
                if (!OverCross)
                {
                    GetComponent<CircleCollider2D>().isTrigger = true;
                    if (!NotDie)
                        Destroy(gameObject, 2f);
                    break;
                }
            }
            if (order[i].transform.TryGetComponent(out Heath h))
            {
                h.enabled = true;
                if (h.name.Contains("drill") || h.name.Contains("panzer"))
                {
                    h.Hit((1.5f - order[i].distance / maxDistance) * 8f * hit_value, _camp);
                    if (Fire)//ве╩П
                    {
                        if (h.TryGetComponent(out OnFire F))
                        {
                            F.value += 0.2f;
                        }
                        else
                        {
                            h.AddComponent<OnFire>().value = 0.2f;
                        }
                    }
                    if (!OverCross)
                    {
                        break;
                    }
                }
                else
                {
                    h.Hit((1.5f - order[i].distance / maxDistance) * 2f * hit_value, _camp);
                    if (Fire)//ве╩П
                    {
                        if (h.TryGetComponent(out OnFire F))
                        {
                            F.value += 0.2f;
                        }
                        else
                        {
                            h.AddComponent<OnFire>().value = 0.2f;
                        }
                    }
                }
            }
        }
        if (last != -1)
        {
            transform.position += transform.right * order[last].distance;
        }
        GetComponent<CircleCollider2D>().isTrigger = true;
        if (!NotDie)
            Destroy(gameObject, 2f);
    }

}
