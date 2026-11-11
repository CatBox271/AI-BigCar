using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject → 组件 的查表，用来替代热路径里的 TryGetComponent（每次都是原生调用）。
/// Heath / StealthSetting 在 Awake 里注册、OnDestroy 里注销。
///
/// 注意：这里刻意不用 OnEnable/OnDisable。Heath.enabled 在游戏过程中会被反复开关
/// （StealthSetting 的隐身、Heath.HitAnim 结束时的 enabled = HP &lt;= 0），
/// 而 TryGetComponent 对 disabled 组件依然有效，用 OnEnable/OnDisable 会导致
/// 行为不一致——被禁用的 Heath 会从查询结果里消失。
/// </summary>
public static class HeathRegistry
{
    static readonly Dictionary<GameObject, Heath> Heaths = new Dictionary<GameObject, Heath>(2048);
    static readonly HashSet<GameObject> Stealths = new HashSet<GameObject>();

    // 关闭 Domain Reload 时静态字段会跨 Play 保留，这里显式清一次
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Heaths.Clear();
        Stealths.Clear();
    }

    public static int Count { get { return Heaths.Count; } }

    public static void Add(Heath h)
    {
        if (h == null) return;
        Heaths[h.gameObject] = h;
    }

    public static void Remove(Heath h)
    {
        if (h == null) return;
        Heaths.Remove(h.gameObject);
    }

    public static void AddStealth(StealthSetting s)
    {
        if (s == null) return;
        Stealths.Add(s.gameObject);
    }

    public static void RemoveStealth(StealthSetting s)
    {
        if (s == null) return;
        Stealths.Remove(s.gameObject);
    }

    public static bool TryGetHeath(Collider2D col, out Heath h)
    {
        return Heaths.TryGetValue(col.gameObject, out h);
    }

    public static bool TryGetHeath(GameObject go, out Heath h)
    {
        return Heaths.TryGetValue(go, out h);
    }

    public static bool IsStealth(Collider2D col)
    {
        return Stealths.Contains(col.gameObject);
    }

    public static bool IsStealth(GameObject go)
    {
        return Stealths.Contains(go);
    }
}
