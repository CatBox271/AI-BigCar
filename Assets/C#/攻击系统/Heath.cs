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
    private int animak = 0;
    private float animation_speed = 5f;

    [System.Flags]
    public enum Effect
    {
        Water = 1,
    }
    public Effect effects;
    public void AddEffect(Effect effect)
    {
        effects |= effect;
    }
    public void DisEffect(Effect effect)
    {
        effects &= ~effect;
    }
    public bool WithEffect(Effect effect)
    {
        return (effects & effect) != 0;
    }
    public void StopBlockAnimation()
    {
        StopCoroutine(nameof(BlockAnimation));
        if(animak == 1) transform.localScale = startScale;
        animak = -1;
    }
    public void StartBlockAnimation(float delay = 0)
    {
        StartCoroutine(BlockAnimation(delay));
    }
    private Vector3 startScale;
    IEnumerator BlockAnimation(float delay)
    {
        if (animak == -1) yield break;
        startScale = transform.localScale;
        animak = 1;
        transform.localScale = new();
        yield return new WaitForSeconds(delay);
        float t = 0;
        do
        {
            t += Time.deltaTime * animation_speed;
            transform.localScale = startScale * t;
            yield return null;
        } while (t < 1f);
        transform.localScale = startScale;
    }

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
        SceneProp sceneProp = SceneProp.Instance;
        bool vm = sceneProp != null && (sceneProp.gameMode == SceneProp.GameMode.video || sceneProp.gameMode == SceneProp.GameMode.card);
        if (vm) IgnoteCampCollider = true;

        camp = _camp;
        max = HP;
        // 阵营染色先算在局部变量里。不能直接写 sp.color：
        // HitLight 打开后 sp.color 归 FlashEffect 的闪烁协程所有，它每帧覆盖，
        // 闪烁结束时还会写回自己那份 originColor。
        Color baseColor = sp != null ? sp.color : Color.white;
        // 层表见 ProjectSettings/TagManager：
        //   本体      中立 6 / 左 7（camp 2 红）/ 右 8（camp 1 蓝）
        //   无队内碰撞 9 / 10 / 11   ← IgnoteCampCollider 时在下面 +3
        //   弹        12 / 13 / 14   ← Bullet 时在下面 +6
        switch (camp)
        {
            case 0:
                gameObject.layer = 6;
                break;
            case 1:

                gameObject.layer = 8;
                if (vm && sp != null && !OrignalColor) baseColor = (Color.cyan + Color.blue) / 2f;
                break;
            case 2:

                gameObject.layer = 7;
                if (vm && sp != null && !OrignalColor) baseColor = Color.red;
                break;
        }
        if (IgnoteCampCollider)
        {
            gameObject.layer += 3;
        }
        else
        {
            if (Bullet)
            {
                gameObject.layer += 6;
            } 
        }
        _color_start = baseColor;
        ApplyBaseColor();
    }

    /// <summary>
    /// 用当前 _color_start 与 HP 算出底色并写出去。
    /// 有 FlashEffect 时必须走 SetOriginColor —— 它持有 sp.color 的所有权，
    /// 直接写 sp.color 会被它的闪烁协程覆盖，闪烁结束后还会写回过期的旧底色
    /// （破碎期间 HP 为负，那份旧底色是负色，表现就是“修完还是黑的”）。
    /// </summary>
    public void ApplyBaseColor()
    {
        if (sp == null) return;
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
        if (HitLight && flash != null) flash.SetOriginColor(a);
        else sp.color = a;
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
                    if (tc.rb_1 != null) tc.rb_1.gameObject.layer = 9;
                    if (tc.rb_2 != null) tc.rb_2.gameObject.layer = 9;
                }
            }
            Die();
        }
        HP -= num;
        if (!DontShow)
        {
            if (showHit != null)
            {
                ShowHit.Spawn(showHit, transform.position, camp, num);
            }
        }
        if (sp == null)
        {
            DH += num;
        }
        else
        {
            // 底色（含 HP 变暗）统一由 ApplyBaseColor 写；有 FlashEffect 时它持有所有权
            ApplyBaseColor();
            if (HitLight && flash != null) flash.WhileBeHit();
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
