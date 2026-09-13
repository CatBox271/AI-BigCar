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
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (sp != null) originColor = sp.color;
    }

    /// <summary>
    /// 更新底色（例如 HP / 护盾值变化后的变暗色）。
    /// 未在闪烁时立即写进 SpriteRenderer；闪烁中则由衰减协程按新底色继续插值，
    /// 所以底色随时可以被外部刷新，不会出现"闪完跳回一个过期颜色"。
    /// </summary>
    public void SetOriginColor(Color c)
    {
        originColor = c;
        if (flashCoroutine == null && sp != null) sp.color = c;
    }

    public void WhileBeHit()
    {
        flashIntensity = Mathf.Min(1, flashIntensity + intensityPerHit);
        if (flashCoroutine == null)
            flashCoroutine = StartCoroutine(FlashWhite());
    }
    public void WhileBeHit(float intensity)
    {
        flashIntensity = Mathf.Min(1, flashIntensity + intensity);
        if (flashCoroutine == null)
            flashCoroutine = StartCoroutine(FlashWhite());
    }
    private IEnumerator FlashWhite()
    {
        while (flashIntensity > 0.001f)
        {
            flashIntensity = Mathf.Max(0, flashIntensity - Time.deltaTime / decayDuration);
            sp.color = Color.Lerp(originColor, Color.white, flashIntensity);
            yield return null;
        }
        flashIntensity = 0;
        sp.color = originColor;
        flashCoroutine = null;
    }
}
