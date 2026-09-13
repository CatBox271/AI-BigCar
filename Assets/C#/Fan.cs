using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    private Rigidbody2D rb;
    public bool On;

    public ParticleSystem ps;

    private float XScale;

    public Transform Rotate;

    public BoxCollider2D bc;

    private Heath h;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Rigidbody2D RB))
        {
            Blow.Add(RB);
        }
    }

    List<Rigidbody2D> Blow = new();


    void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out h);
        XScale = Rotate.localScale.x;
    }

    public float Force;
    public float MaxSpeed;


    public float D = 2f;

    void FixedUpdate()
    {
        if (h.HP == 0) return;
        for (int i = 0; i < Blow.Count; i++)
        {
            Rigidbody2D RB = Blow[i];
            if (RB == null) continue;
            float WindLength = Mathf.Pow(AnimSpeed, 0.5f) * 0.75f;
            RB.AddForce(transform.up / Mathf.Pow((RB.position - rb.position).magnitude, 2f) * AnimSpeed / Blow.Count);
        }

        Blow = new();
        if (On)
        {





            Vector2 sp = rb.velocity;

            float MS = MaxSpeed;
            float F = Force;
            Vector2 ff = Mathf.Max(0.5f, (1 - Vector2.Dot(transform.up, -sp) / MS)) * -F * transform.up;

            AnimSpeed = ff.magnitude;
            rb.AddForce(ff);
            bc.enabled = true;
            float Lenth = Mathf.Pow(AnimSpeed, 0.5f) * 0.75f;
            bc.size = new(1, Lenth);
            bc.offset = new(0, Lenth / 2f + 0.27f);




            RaycastHit2D rh = Physics2D.Linecast(transform.position, transform.position + transform.up * Mathf.Pow(AnimSpeed, 0.5f) * 0.75f,1);
            if (rh.collider != null)
            {
                float Closet = (rh.point - rb.position).magnitude;
                rb.AddForce(transform.up * -0.75f * Mathf.Max(0,(Mathf.Pow( Mathf.Cos(Mathf.Clamp(Closet /2f,0 ,1)),3)) * Force));
                tt++;
                if (tt > 45)
                {
                    tt = 0;
                    if (rh.collider.gameObject.TryGetComponent(out WaterWave ww))
                    {
                        ww.AddNewPoint(rh.point, (Lenth - Closet) * AnimSpeed / -600f);
                    }
                }
            }

        }
        else
        {
            bc.enabled = false;
        }
    }
    int tt = 0;
    float AnimSpeed = 0;
    float angle = 0;

    [System.Obsolete]
    private void Update()
    {
        if (!On)
        {
            ps.gameObject.SetActive(false);
        }
        else
        {
            ps.gameObject.SetActive(true);
            angle -= AnimSpeed * 4 * Time.deltaTime;
            Rotate.localScale = new Vector3(Mathf.Cos(angle) * XScale, Rotate.localScale.y, Rotate.localScale.z);
            ps.startSpeed = Mathf.Pow(AnimSpeed,0.5f );
        }
        Sound(On);
    }

    public AudioClip[] ac;
    public float volume;

    AudioSource audioSound;
    public void Sound(bool On)
    {
        if (ac.Length != 0)
        {
            if (audioSound == null)
            {
                GameObject go = Instantiate(VanishWhenAudioDone.StaticAudio, transform);
                go.transform.localPosition = new();
                go.transform.localEulerAngles = new();
                audioSound = go.GetComponent<AudioSource>();
                audioSound.clip = ac[UnityEngine.Random.Range(0, ac.Length)];
                audioSound.loop = true;
                audioSound.Play();
            }
            else
            {
                audioSound.volume = On ? volume : 0;
            }
        }
    }
}
