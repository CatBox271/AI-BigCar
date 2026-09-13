using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPG : MonoBehaviour
{
    public ParticleSystem ps;
    public GameObject TNT;
    public float CD;
    public bool start;
    private Heath h;
    public bool Crash;
    private Rigidbody2D rb;
    public float Radius;
    public float Hit;
    public bool enable;


    private void Awake()
    {
        TryGetComponent(out h);
        TryGetComponent(out rb);
        StartCoroutine(Copy());
    }
    Vector3 aim = Vector3.zero;
    private bool FindEnemy()
    {

        Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position,10f);
        float _dis = 11f;
        Vector3 _pos = new(0, 0, 0);
        for (int i = 0; i < collider2D.Length; i++)
        {
            if (collider2D[i].TryGetComponent(out StealthSetting _)) continue;
            if (!collider2D[i].TryGetComponent<Heath>(out var hj)) continue;
            if (hj.camp == h.camp) continue;
            if (hj.die) continue;
            Vector2 distanceBetween = transform.position - collider2D[i].transform.position;
            float _newdis = distanceBetween.magnitude;
            if (_newdis < _dis)
            {
                _pos = collider2D[i].transform.position;
                _dis = _newdis;
            }
        }
        if (_pos != Vector3.zero)
        {
            aim = _pos;
            return true;
        }
        return false;
    }

    IEnumerator Fire()
    {
        ps.Play();
        if (TryGetComponent(out TryConnect ty))
        {
            Destroy(ty);
        }
        if (TryGetComponent(out RelativeJoint2D rj))
        {
            Destroy(rj);
        }
        float time = Time.time;
        do
        {
            rb.velocity = transform.up * 10f;
            
            
            
            
            
            ;
        } while (Time.time - time < 0.6f && !Crash && transform.position.y < 4f);
        transform.localEulerAngles = new Vector3(0, 0, h.camp == 1?-90f:90f);
        time = Time.time;
        do
        {
            rb.angularVelocity = 0f;
            rb.velocity = transform.up * 20f;
            yield return null;
        } while (Time.time - time < 2f && !Crash && !FindEnemy());
        time = Time.time;
        if (aim != Vector3.zero)
        {
            Vector3 v = aim - transform.position;
            v.z = 0;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.left, v);
            transform.rotation = rotation;
            transform.localEulerAngles += new Vector3(0, 0, 90f);
        }
        do
        {
            rb.angularVelocity = 0f;
            rb.velocity = transform.up * 20f;
            yield return null;
        } while (!Crash && Time.time - time < 1f);
        Boom();
        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (start)
        {
            Crash = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (start)
        {
            if (collision.CompareTag("EnergyShield"))
            {
                if (collision.transform.parent.TryGetComponent(out Shield _))
                {
                    Hit *= 0.25f;
                }
            }

        }
    }
    private void Boom()
    {
        Color col = Color.white;
        int lay = 0;
        if (h.camp == 1)
        {
            col = new Color(1f, 0.5f, 0.5f);
        }
        if (h.camp == 2)
        {
            col = new Color(0.5f, 0.5f, 1f);
        }
        Instantiate(TNT, transform.position, transform.rotation).GetComponent<TNTWave>().Setting(col, lay, Radius, 6f, Hit, h.camp);
        Destroy(gameObject);
    }
    IEnumerator Copy()
    {
        yield return new WaitUntil(() => enable);
        yield return null;
        yield return new WaitUntil(() => h.camp != 0);
        if (!start)
        {
            do
            {
                GameObject go = Instantiate(gameObject, transform.parent);
                go.GetComponent<RPG>().start = true;
                yield return new WaitForSeconds(CD);
            } while (!h.die && !start);
        }
        else
        {
            StartCoroutine(Fire());
        }
    }


}
