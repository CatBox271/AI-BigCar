
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
        float queryRadius = Radius + (fly ? 10f : 0f);

        // 共享范围查询：同一格子的炮塔复用同一份候选集（见 RangeQueryCache）。
        // 每个开火周期仍然重新选目标，所以转火行为与原来一致。
        Collider2D[] collider2D;
        int hitCount = RangeQueryCache.Get(transform.position, queryRadius, camp, out collider2D);

        float _dis = Radius+1;
        Transform _pos = null;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = collider2D[i];
            // 共享缓存会保留引用到刷新间隔结束，期间目标可能已被销毁；
            // 用 Unity 的假空判断（== null）跳过，这是 C 方案引入的必要防护
            if (col == null) continue;
            if (!Castle && HeathRegistry.IsStealth(col)) continue;
            if (!HeathRegistry.TryGetHeath(col, out Heath h)) continue;
            if (Castle && (h.gameObject.layer == 8 || h.gameObject.layer == 9)) continue;
            if (h.camp == camp) continue;
            if (h.HP<=0 && !h.CompareTag("Site")) continue;
            Vector2 distanceBetween = transform.position - col.transform.position;
            float _newdis = distanceBetween.magnitude;

            if (_newdis < _dis)
            {
                _pos = col.transform;
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
            go.layer = 14;
        }
        else
        {
            if (camp == 0)
            {
                go.layer = 12;
            }
            else
            {
                go.layer = 13;
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

    bool LookToEnemy(Transform pos,bool H)
    {
        if (pos != null)
        {
            if (H)
            {
                // 刚体与相对速度每次瞄准只解析一次（原先在迭代里重复 5~20 次原生查找）
                Rigidbody2D aimRb = pos.GetComponentInParent<Rigidbody2D>();
                Rigidbody2D selfRb = rb;
                if (selfRb == null && transform.parent != null) selfRb = transform.parent.GetComponentInParent<Rigidbody2D>();
                if (selfRb == null) selfRb = GetComponentInParent<Rigidbody2D>();

                float px = pos.position.x - transform.position.x;
                float py = pos.position.y - transform.position.y;
                float vrx = (aimRb != null ? aimRb.velocity.x : 0f) - (selfRb != null ? selfRb.velocity.x : 0f);
                float vry = (aimRb != null ? aimRb.velocity.y : 0f) - (selfRb != null ? selfRb.velocity.y : 0f);

                float angle;
                bool answer = BallisticMath.Solve(px, py, vrx, vry, _bulletSpeed, Gravity, out _, out angle)
                              && !float.IsNaN(angle) && !float.IsInfinity(angle);

                if (answer)
                {
                    transform.eulerAngles = new Vector3(0, 0, angle);
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
            // 复用音效对象，避免每发子弹都 Instantiate 一个带 AudioSource 的物体
            VanishWhenAudioDone.Play(transform, ac[Random.Range(0, ac.Length)], volume);
        }
    }
}
