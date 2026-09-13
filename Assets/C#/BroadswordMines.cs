using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadswordMines : MonoBehaviour
{
    public GameObject _bullet;
    public float Radius;
    public int Num;
    public float Hit;
    public float MaxDistance;
    bool flip;
    Heath health;
    private void Start()
    {
        TryGetComponent(out health);
    }
    public void QD()
    {
        st = true;
    }
    public bool st;

    private void FixedUpdate()
    {
        if (!st) return;
        if (!flip && health.camp == 2)
        {
            flip = Camera.main.name != "VideoMode" ;
            GetComponent<SpriteRenderer>().flipX = Camera.main.name != "VideoMode";
        }
        if (Find())
        {
            Fire();
        }
    }
    void Fire()
    {
        for (int i = 0; i < Num; i++)
        {
            GameObject go = Instantiate(_bullet, transform.parent);
            go.transform.position = transform.position + new Vector3(0.25f * (flip ? -1f : 1f), Random.Range(-0.075f, 0.075f));

            Vector3 v = go.transform.position - transform.position;
            v.z = 0;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, v); 
            go.transform.rotation = rotation;
            go.layer = gameObject.layer + 2;
            bullet b =
            go.GetComponent<bullet>();
            b.SetDistance(MaxDistance);
            b.Gravity = 9.81f;
            b.SetInform(health.camp, 50f+Random.Range(-10f, 10f), Hit, Color.white);

        }
        try
        {
            transform.parent.GetChild(0).GetComponent<Rigidbody2D>().velocity -= 50f * new Vector2(transform.right.x, transform.right.y) * (flip ? -1 : 1);
            print(transform.parent.GetChild(0));
        }
        catch
        { 
        
        }
        Destroy(gameObject);
    }

    bool Find()
    {
        List<RaycastHit2D> ray = new();
        ray.AddRange(Physics2D.LinecastAll(transform.position, transform.position + (health.camp == 1 ? transform.right * Radius : -transform.right * Radius)));
        for (int i = 0; i < ray.Count; i++)
        {
            if ((ray[i].transform.CompareTag("EnergyShield") && ray[i].transform.parent.GetComponent<Heath>().camp != health.camp || ray[i].transform.TryGetComponent(out Heath h) && h.camp != health.camp))
            {
                return true;
            }
        }
        return false;
    }
}
