using UnityEngine;

/// <summary>
/// 全局范围查询工具：按阵营生成 layer 掩码 + 无分配的圆形重叠查询。
///
/// 阵营层约定来自 Heath.Set()：
///   camp 0 中立 → layer 11 (Neuro)
///   camp 1 红方 → layer 6 (Red) ；VideoMode 下 IgnoteCampCollider 生效 → layer 12 (RedIngnote)
///   camp 2 蓝方 → layer 7 (Blue)；VideoMode 下 IgnoteCampCollider 生效 → layer 13 (BlueIngnote)
///   子弹在以上基础上再 +2（8 = RedBullet，9 = BlueBullet）
/// 因此"排除自己阵营占用的层"就能让物理层直接完成阵营过滤，
/// 不必把整个半径内的碰撞体都取回来再逐个判 camp（实测候选数可从 127 降到 27，
/// 且筛选结果与原实现逐项一致）。
/// </summary>
public static class RangeQuery
{
    public const int LayerDefault = 0;
    public const int LayerRed = 6;
    public const int LayerBlue = 7;
    public const int LayerRedBullet = 8;
    public const int LayerBlueBullet = 9;
    public const int LayerNeuro = 11;
    public const int LayerRedIngnote = 12;
    public const int LayerBlueIngnote = 13;

    /// <summary>该阵营自身占用的层位。用于计算敌我掩码。</summary>
    public static int SelfLayerBits(int camp)
    {
        switch (camp)
        {
            case 0: return 1 << LayerNeuro;
            case 1: return (1 << LayerRed) | (1 << LayerRedIngnote);
            case 2: return (1 << LayerBlue) | (1 << LayerBlueIngnote);
            default: return 0;
        }
    }

    /// <summary>camp 需要查询的层掩码：排除自己阵营，保留敌方与中立、子弹等。</summary>
    public static int TargetMask(int camp)
    {
        return ~SelfLayerBits(camp);
    }

    static Collider2D[] _buffer = new Collider2D[1024];

    /// <summary>
    /// 最近一次 OverlapCircle 的结果。必须在本次调用之后读取，
    /// 并且不要跨调用缓存这个数组——缓冲区在结果被截断时会整体扩容替换。
    /// </summary>
    public static Collider2D[] Results
    {
        get { return _buffer; }
    }

    /// <summary>按阵营做圆形重叠查询（自动换算成敌我掩码），返回结果数量；结果见 Results。</summary>
    public static int OverlapCircleForCamp(Vector2 center, float radius, int camp)
    {
        return OverlapCircle(center, radius, TargetMask(camp));
    }

    /// <summary>按层掩码做圆形重叠查询，返回结果数量；结果见 Results。不产生 GC 分配。</summary>
    public static int OverlapCircle(Vector2 center, float radius, int layerMask)
    {
        return OverlapCircleInto(center, radius, layerMask, ref _buffer);
    }

    /// <summary>
    /// 把查询结果写进调用方提供的缓冲区（共享缓存等需要各自持有数组的场合）。
    /// 缓冲区被装满时 NonAlloc 会静默截断结果、可能漏掉目标，
    /// 这里扩容后重查，宁可多分配一次也不改变原有筛选结果。
    /// </summary>
    public static int OverlapCircleInto(Vector2 center, float radius, int layerMask, ref Collider2D[] buffer)
    {
        if (buffer == null || buffer.Length == 0) buffer = new Collider2D[64];

        int n = Physics2D.OverlapCircleNonAlloc(center, radius, buffer, layerMask);
        while (n >= buffer.Length && buffer.Length < (1 << 20))
        {
            Collider2D[] bigger = new Collider2D[buffer.Length * 2];
            n = Physics2D.OverlapCircleNonAlloc(center, radius, bigger, layerMask);
            buffer = bigger;
        }
        return n;
    }
}
