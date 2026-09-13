
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Artillery : MonoBehaviour
{
    public int camp;
    public float coolTime;
    public float Radius;
    public float Hit;
    public GameObject bullet;
    public float CD;
    public bool BulletMaxDistance;
    public bool Castle;
    public bool Laser;
    public float Gravity;
    public float _bulletSpeed = 20f;
    public ColorChangeWith Barrel;
    public int count;

    float InsideClock = 0;

    void FixedUpdate()
    {
        
        CD -= Time.fixedDeltaTime;
        if (Barrel != null)
        {
            Barrel.light_value = (Mathf.Clamp((coolTime - CD) / coolTime, 0, 1) * 1.25f - 0.9f);
        }

        if (CD <= 0)
        {
            InsideClock -= Time.fixedDeltaTime;
            if (InsideClock > 0) return;
            InsideClock = coolTime;
            Transform aimpos = FindEnemy();
            if (aimpos != null)//Find enemy
            {

                if (LookToEnemy(aimpos, !Laser))
                {
                    Fire();
                    CD = coolTime;
                    InsideClock = 0;
                    count++;
                }
            }
            else
            {
                LookToEnemy(null, false);
            }
        }
    }
    private Transform FindEnemy()
    {
        bool fly = transform.position.y > 0;

        Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position, Radius + (fly ? 10f : 0f));

        float _dis = Radius+1;
        Transform _pos = null;
        for (int i = 0; i < collider2D.Length; i++)
        {
            if (collider2D[i].TryGetComponent(out StealthSetting _) && !Castle) continue;
            if (!collider2D[i].TryGetComponent<Heath>(out var h)) continue;
            if (Castle && (h.gameObject.layer == 8 || h.gameObject.layer == 9)) continue;
            if (h.camp == camp) continue;
            if (h.HP<=0 && !h.CompareTag("Site")) continue;
            Vector2 distanceBetween = transform.position - collider2D[i].transform.position;
            float _newdis = distanceBetween.magnitude;

            if (_newdis < _dis)
            {
                _pos = collider2D[i].transform;
                _dis = _newdis;
            }
        }
        return _pos;
    }
    private void Fire()
    {
        Sound();
        GameObject go = Instantiate(bullet, transform.position, transform.rotation);
        Color give_color = Color.white;
        if (camp == 1)
        {
            give_color = new Color(0.5f, 0.5f, 1f);
            go.layer = 8;
        }
        else
        {
            if (camp == 0)
            {
                go.layer = 11;
            }
            else
            {
                go.layer = 9;
                give_color = new Color(1f, 0.5f, 0.5f);

            }
        }
        bullet _bullet = go.GetComponent<bullet>();

        if (_bullet.TryGetComponent(out Rigidbody2D _rb))
        {
            if (rb != null) _rb.velocity = rb.velocity;
        }

        _bullet.Laser = Laser;
        _bullet.Gravity = Gravity;
        if (BulletMaxDistance)
        {
            _bullet.SetDistance(Radius + 0.5f);
        }
        _bullet.SetInform(camp, _bulletSpeed, Hit, give_color);

    }
    public Rigidbody2D rb;

    (float,float) XYT(Transform Self, Transform Aim,float T)
    {
        Rigidbody2D AimRb = Aim.GetComponent<Rigidbody2D>();

        Rigidbody2D SelfRb = Self.parent.GetComponent<Rigidbody2D>();
        if (rb != null) SelfRb = rb;
        float DX = (Aim.position.x - Self.position.x) + (AimRb.velocity.x - SelfRb.velocity.x) * T;
        float DY = (Aim.position.y - Self.position.y) + (AimRb.velocity.y - SelfRb.velocity.y) * T;
        float DY2 = DY * DY;
        float DX2 = DX * DX;
        float V = _bulletSpeed;
        float V2 = V * V;

        float A = 0.25f * Gravity * Gravity;
        float B = -(DY * -Gravity + V2);
        float C = DY2 + DX2;
        float Delta = B * B - 4 * A * C;


        float T2 = float.NaN;
        if (Delta > 0)
        {
            T2 = (-B + Mathf.Sqrt(Delta)) / (2 * A);
            float compare = (-B - Mathf.Sqrt(Delta)) / (2 * A);

            if (compare < T2)
            {
                if (compare > 0)
                {
                    T2 = compare;
                }
            }
            if (T2 < 0)
            {
                T2 = float.NaN;
            }
        }
        float cos¦È = DX / V / T;
        float sin¦È = (DY - 0.5f * -Gravity * T * T) / V / T;
        float ¦È = Mathf.Rad2Deg * Mathf.Acos(cos¦È);
        if (sin¦È < 0)
        {
            ¦È *= -1f;
        }
        return (Mathf.Sqrt(T2) , ¦È);
    }

    public int CalculateTimes = 5;
    bool LookToEnemy(Transform pos,bool H)
    {
        if (pos != null)
        {
            if (H)
            {
                float T = 0;
                float ¦È = 0;
                float Last¦È = 0;
                for (int t = 0; t < CalculateTimes; t++)
                {
                    Last¦È = ¦È;
                    (float, float) ff = XYT(transform, pos, T);
                    T = ff.Item1;
                    ¦È = ff.Item2;
                    if (T == float.NaN || ¦È == float.NaN)
                    {
                        T = -1 * (t + 1);
                        break;
                    }
                }
                bool answer = false;
                if (T < 0)
                {
                    if (T != -1)
                    {
                        ¦È = Last¦È;
                        answer = true;
                    }
                    else
                    {
                        answer = false;
                    }
                }
                else
                {
                    answer = true;

                }
                if (!float.IsNormal(¦È)) answer = false;
                if (answer)
                {
                    transform.eulerAngles = new Vector3(0, 0, ¦È);
                }
                else
                {
                    transform.localEulerAngles = new Vector3(0, 0, camp == 2 ? 180 : 0);
                }
                return answer;
            }
            else
            {
                Vector3 v = pos.position - transform.position;
                v.z = 0;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.right, v);
                transform.rotation = rotation;
                return true;
            }
        }
        else
        {
            w_r_t++;
            if (w_r_t > 30)
            {
                w_r_t = 0;
                wait_aim = Random.Range(-10f, 10f);
            }
            wait_angle = Mathf.Lerp(wait_angle, wait_aim, 0.1f);
            transform.eulerAngles = new Vector3(0, 0, (camp == 2 ^ IdleOpposite ? 180 : 0) + wait_angle);
            return false;
        }
    }
    float wait_aim = 0;
    float wait_angle = 0;
    int w_r_t = 0;


    public AudioClip[] ac;
    public float volume;

    public bool IdleOpposite;
    public void Sound()
    {
        if (ac.Length != 0)
        {
            if (VanishWhenAudioDone.StaticAudio != null)
            {
                GameObject go = Instantiate(VanishWhenAudioDone.StaticAudio, transform);
                go.transform.localPosition = new();
                go.transform.localEulerAngles = new();
                AudioSource audio = go.GetComponent<AudioSource>();
                audio.clip = ac[Random.Range(0, ac.Length)];
                audio.volume = volume;
                audio.Play();
            }
        }
    }
}
