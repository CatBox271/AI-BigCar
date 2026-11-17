using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按 interval 分组的节拍。同一个 interval 只维护一份累加，所有用它的人共享同一帧的 Due 结果，
/// 不各自 += dt 再比较。取一次引用放字段里，每帧只读一个 bool。
/// </summary>
public sealed class ClockBucket
{
    internal float interval;
    internal float accum;
    /// <summary>本帧是否到期</summary>
    public bool Due { get; internal set; }
    /// <summary>到期回调，参数是本次覆盖的时间。只有到期那一帧才会被调用，不是每帧。</summary>
    public event System.Action<float> OnDue;

    /// <summary>派发到期回调。event 只能在声明类型内 invoke，所以由 Clock 调这个。</summary>
    internal void Fire()
    {
        OnDue?.Invoke(Passed);
    }
    /// <summary>本次到期实际覆盖的时间（可能大于 interval），按它算步长渐变曲线就不会被频率影响</summary>
    public float Passed { get; internal set; }
}

/// <summary>
/// 全局时钟：一个 Update 推进所有节拍，其他系统要用低频刷新就 Clock.Bucket(间隔) 拿一个桶。
/// </summary>
public static class Clock
{
    static readonly List<ClockBucket> buckets = new();
    static readonly Dictionary<float, ClockBucket> lookup = new();
    static ClockRunner runner;

    public static float Delta { get; private set; }
    public static float Now { get; private set; }
    public static int Frame { get; private set; }

    /// <summary>拿一个节拍桶。interval 相同的调用拿到的是同一个桶。</summary>
    public static ClockBucket Bucket(float interval)
    {
        if (lookup.TryGetValue(interval, out ClockBucket b)) return b;
        b = new ClockBucket { interval = interval };
        lookup[interval] = b;
        buckets.Add(b);
        return b;
    }

    /// <summary>
    /// 驱动在场景加载后建。不能放在 Bucket() 里：静态字段初始化时可能正在 MonoBehaviour 的构造/字段初始化上下文，
    /// 那里绝对不允许 CreateGameObject（UnityException: Internal_CreateGameObject is not allowed ...）。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        EnsureRunner();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>Clock 物体跟着场景销毁，切场景后要重新建一个。</summary>
    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        EnsureRunner();
    }

    static void EnsureRunner()
    {
        if (runner != null) return;
        GameObject go = new GameObject("Clock");
        runner = go.AddComponent<ClockRunner>();
    }

    internal static void Tick(float dt)
    {
        Delta = dt;
        Now += dt;
        Frame++;
        for (int i = 0; i < buckets.Count; i++)
        {
            ClockBucket b = buckets[i];
            b.accum += dt;
            if (b.accum >= b.interval)
            {
                b.Passed = b.accum;
                b.accum = 0f;
                b.Due = true;
                b.Fire();//只在到期这一帧派发
            }
            else
            {
                b.Due = false;
            }
        }
    }
}
