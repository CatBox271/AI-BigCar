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
    public bool OrignalColor;

    public bool Castle;

    public bool DieWithCollider;

    public bool IgnoteCampCollider;

    private void Awake()
    {
        max = HP;
        TryGetComponent(out sp);
        if (Castle)
        {
            Set(camp);
        }
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

    private bool HitAnimFinish = true;
    private Color _lightPercent;

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
            Color a = _color_start + _lightPercent;
            if (max != 0)
            {
                a *= HP / max;
                a.a = 1f;
            }
            else
            {
                a = Color.black;
            }
            if (sp != null) sp.color = a;
            if (HitLight && HitAnimFinish)
            {
                StartCoroutine("HitAnim");
                return;
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

    IEnumerator HitAnim()
    {
        HitAnimFinish = false;

        _lightPercent = new Color(0.4f, 0.4f, 0.4f, 0);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 5; i++)
        {
            _lightPercent -= new Color(0.4f, 0.4f, 0.4f, 0) / 5f;
            yield return null;
        }
        _lightPercent = Color.clear;
        yield return new WaitForSeconds(0.1f);
        HitAnimFinish = true;
        enabled = HP<= 0;
    }
}
