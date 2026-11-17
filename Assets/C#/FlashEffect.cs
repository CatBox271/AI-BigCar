using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    public SpriteRenderer sp;
    private Color originColor;

    [Range(0, 1)] public float intensityPerHit = 0.4f;
    public float decayDuration = 0.3f;
    private float flashIntensity;
    private bool flashing;

    private void Awake()
    {
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (sp != null) originColor = sp.color;
    }


    /// <summary>
    /// 更新底色（例如 HP / 护盾值变化后的变暗色）。
    /// 未在闪烁时立即写进 SpriteRenderer；闪烁中则由 FlashDriver 按新底色继续插值，
    /// 所以底色随时可以被外部刷新，不会出现"闪完跳回一个过期颜色"。
    /// </summary>
    public void SetOriginColor(Color c)
    {
        originColor = c;
        if (!flashing && sp != null) sp.color = c;
    }

    public void WhileBeHit()
    {
        flashIntensity = Mathf.Min(1, flashIntensity + intensityPerHit);
        if (!flashing)
        {
            flashing = true;
            FlashDriver.Register(this);
        }
    }
    public void WhileBeHit(float intensity)
    {
        flashIntensity = Mathf.Min(1, flashIntensity + intensity);
        if (!flashing)
        {
            flashing = true;
            FlashDriver.Register(this);
        }
    }

    /// <summary>
    /// 由 FlashDriver 每帧推进一次。返回 false 表示闪完了，可以从名单里摘掉。
    /// </summary>
    public bool StepFlash(float dt)
    {
        if (sp == null)
        {
            flashing = false;
            return false;
        }
        if (flashIntensity <= 0.001f)
        {
            flashIntensity = 0f;
            sp.color = originColor;
            flashing = false;
            return false;
        }
        flashIntensity = Mathf.Max(0, flashIntensity - dt / decayDuration);
        if (flashIntensity <= 0.001f)
        {
            flashIntensity = 0f;
            sp.color = originColor;
            flashing = false;
            return false;
        }
        sp.color = Color.Lerp(originColor, Color.white, flashIntensity);
        return true;
    }
}

/// <summary>
/// 所有受击闪烁的统一驱动。
/// 以前每个 FlashEffect 各起一个协程，一次大爆炸同帧几百个物体挨打就是几百个协程对象 + 状态机（GC 大头）。
/// 现在只有一个常驻的 FlashDriver 物体，每帧倒序遍历活跃名单；名单和驱动对象都是复用，不按次分配。
/// 这个物体是普通物体：Hierarchy 里看得见，没设 hideFlags，也不做 SetActive(false)。
/// </summary>
public static class FlashDriver
{
    static readonly List<FlashEffect> active = new();
    static FlashDriverRunner driver;

    /// <summary>第一次有人要闪时建出来；切场景被销毁后，下次有人要闪会重新建。</summary>
    static void EnsureDriver()
    {
        if (driver != null) return;//Unity 的伪 null：物体被销毁时这里会重建
        GameObject go = new GameObject("FlashDriver");
        driver = go.AddComponent<FlashDriverRunner>();
    }

    public static void Register(FlashEffect f)
    {
        if (f == null) return;
        EnsureDriver();
        if (!active.Contains(f)) active.Add(f);
    }

    public static void Tick(float dt)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            FlashEffect f = active[i];
            if (f == null)
            {
                active.RemoveAt(i);
                continue;
            }
            if (!f.StepFlash(dt)) active.RemoveAt(i);
        }
    }
}
