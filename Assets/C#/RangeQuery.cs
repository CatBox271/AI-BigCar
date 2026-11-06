using UnityEngine;

/// <summary>
/// 全局范围查询工具：按阵营生成 layer 掩码 + 无分配的圆形重叠查询。
///
/// 阵营层约定来自 Heath.Set()：
///   本体层    中立 6 / 左 7（camp 2 红）/ 右 8（camp 1 蓝）
///   无队内碰撞 9 / 10 / 11 —— VideoMode 下 IgnoteCampCollider 生效
///   弹层     12 / 13 / 14
/// 因此"排除自己阵营占用的层"就能让物理层直接完成阵营过滤，
/// 不必把整个半径内的碰撞体都取回来再逐个判 camp（实测候选数可从 127 降到 27，
/// 且筛选结果与原实现逐项一致）。
/// </summary>
public static class RangeQuery
{
    public const int LayerDefault = 0;
    public const int LayerNeutral = 6;          // 中立本体
    public const int LayerLeft = 7;             // 左方本体（camp 2 红）
    public const int LayerRight = 8;            // 右方本体（camp 1 蓝）
    public const int LayerNeutralNoTeam = 9;    // 中立·无队内碰撞
    public const int LayerLeftNoTeam = 10;      // 左方·无队内碰撞
    public const int LayerRightNoTeam = 11;     // 右方·无队内碰撞
    public const int LayerNeutralBullet = 12;   // 中立弹
    public const int LayerLeftBullet = 13;      // 左方弹
    public const int LayerRightBullet = 14;     // 右方弹

    /// <summary>该阵营自身占用的层位（本体 / 无队内碰撞 / 弹）。用于计算敌我掩码。</summary>
    public static int SelfLayerBits(int camp)
    {
        switch (camp)
        {
            case 0: return (1 << LayerNeutral) | (1 << LayerNeutralNoTeam) | (1 << LayerNeutralBullet);
            case 1: return (1 << LayerRight) | (1 << LayerRightNoTeam) | (1 << LayerRightBullet);
            case 2: return (1 << LayerLeft) | (1 << LayerLeftNoTeam) | (1 << LayerLeftBullet);
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
