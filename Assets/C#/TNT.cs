using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour,ION
{
    private Heath heath;
    public GameObject BoomWave;

    public float Radius = 5.5f;
    public float Hit = 10f;
    bool Video;
    private void Awake()
    {
        TryGetComponent(out heath);
    }
    private void Start()
    {
        Video = Camera.main.name == "VideoMode";
    }


    bool Find;
    public bool Fly;

    public bool OnSet { get; set; }
    public float NumSet { get; set; }

    private void Update()
    {
        if (OnSet)
        {
            Boom();
            return;
        }
        if (!Video) return;
        if (transform.position.y > 10f)
        {
            Fly = true;
        }
        else
        {
            Fly = false;
        }
        if (Fly && !Find)
        {
            RaycastHit2D[] line = Physics2D.LinecastAll(transform.position, transform.position - new Vector3(0, 13));
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].transform.TryGetComponent(out Heath h))
                {
                    if (h.camp != heath.camp && h.HP > 0)
                    {
                        Find = true;
                        if (TryGetComponent(out RelativeJoint2D rj))
                        {
                            Destroy(rj);
                        }
                        heath.KeepRecord[0] += 5;
                        if (TryGetComponent(out Rigidbody2D rb)) rb.velocity = new(0, -20f);
                    }
                }
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Find && !collision.gameObject.name.Contains("Bullet"))
        {
            Boom();
        }
    }

    private void OnDestroy()
    {
        Boom();
    }

    public void Boom()
    {
        Color col = Color.white;
        int lay = 0;
        if (heath.camp == 1)
        {
            col = new Color(0.75f, 0.75f, 1f);
        }
        if (heath.camp == 2)
        {
            col = new Color(1f, 0.75f, 0.75f);
        }
        Instantiate(BoomWave, transform.position, transform.rotation).GetComponent<TNTWave>().Setting(col, lay, Radius, 30f, Hit, heath.camp);
        Destroy(gameObject);
    }
}
