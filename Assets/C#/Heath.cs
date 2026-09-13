using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heath : MonoBehaviour
{
    public int camp;
    public float HP;
    public float max;
    private SpriteRenderer sp;
    public Color _color_start;
    public bool die;

    public bool HitLight;

    [Tooltip("受击变亮效果。HitLight 打开时由 Awake 自动挂载")]
    public FlashEffect flash;

    public bool OrignalColor;

    public bool Castle;

    public bool DieWithCollider;

    public bool IgnoteCampCollider;

    private void Awake()
    {
        max = HP;
        TryGetComponent(out sp);
        HeathRegistry.Add(this);
        if (HitLight)
        {
            flash = GetComponent<FlashEffect>();
            if (flash == null) flash = gameObject.AddComponent<FlashEffect>();
        }
        if (Castle)
        {
            Set(camp);
        }
    }

    private void OnDestroy()
    {
        HeathRegistry.Remove(this);
    }
    public bool Bullet;

    public void ND()
    {
        NotDisappear = true;
    }
    private bool NotDisappear;
    public void Set(int _camp)
    {
        bool vm = Camera.main.name == "VideoMode";
        if (vm) IgnoteCampCollider = true;

        camp = _camp;
        max = HP;
        switch (camp)
        {
            case 0:
                gameObject.layer = 11;
                break;
            case 1:

                gameObject.layer = 6;
                if (vm && sp != null && !OrignalColor) sp.color = (Color.cyan + Color.blue) / 2f;
                break;
            case 2:

                gameObject.layer = 7;
                if (vm && sp != null && !OrignalColor) sp.color = Color.red;
                break;
        }
        if (IgnoteCampCollider && camp != 0)
        {
            gameObject.layer += 6;
        }
        else
        {
            if (Bullet)
            {
                gameObject.layer += 2;
            } 
        }
        if(sp != null) _color_start = sp.color;
    }

    public GameObject showHit;

    public float DH;

    public List<float> KeepRecord = new() {0,0,0};


    public float MaxHit = 0;
    public float MaxHitLimit = 0;
    public float OutHitTimes = 1;
    public void Hit(float num, int camp,bool DontShow = false)
    {
        if (MaxHit > 0 && num > MaxHit)
        {
            if (num <= MaxHitLimit)
            {
                num = MaxHit;
            }
            else
            {
                num = OutHitTimes * (num - MaxHitLimit) + MaxHit;
            }
        }
        if (Castle) num = Mathf.Min(num, 100f);
        GetHitTimes++;
        KeepRecord[camp] += num;
        if (HP <= num)
        {
            if (!DieWithCollider)
            {
                if (TryGetComponent(out TryConnect tc))
                {
                    if (tc.rb_1 != null) tc.rb_1.gameObject.layer = 11;
                    if (tc.rb_2 != null) tc.rb_2.gameObject.layer = 11;
                }
            }
            Die();
        }
        HP -= num;
        if (!DontShow)
        {
            if (showHit != null)
            {
                Instantiate(showHit, transform.position, new Quaternion()).GetComponent<ShowHit>().Setting(camp, num);
            }
        }
        if (sp == null)
        {
            DH += num;
        }
        else
        {
            Color a = _color_start;
            if (max != 0)
            {
                a *= HP / max;
                a.a = 1f;
            }
            else
            {
                a = Color.black;
            }
            if (HitLight && flash != null)
            {
                // 底色（含 HP 变暗）交给 FlashEffect 统一持有，
                // 由它负责受击变亮，以及闪烁结束后写回底色
                flash.SetOriginColor(a);
                flash.WhileBeHit();
            }
            else
            {
                sp.color = a;
            }
        }
    }
    public void Die()
    {
        if (NotDisappear)
        {
            return;
        }
        Destroy(gameObject);
    }

    public int GetHitTimes = 0;
}
