using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 共享范围查询缓存（C 方案）。
///
/// 目的：同一片区域里的多个炮塔问的几乎是同一个问题，没必要每人各查一次物理。
/// 做法：把世界划成网格，以 (格子, 阵营) 为单位缓存一次查询结果，有效期为
/// RefreshInterval 秒；同一格内的炮塔共用这份候选集，再各自按自己的半径过滤。
///
/// 等价性：缓存查询以格中心为心、半径 = 半对角线 + 格内最大请求半径，
/// 因此格内任一炮塔的圆形查询范围都被它完全包含，结果是各炮塔直接查询结果的超集；
/// 各炮塔再按自身半径做最近邻筛选，结论与直接查询一致。
///
/// 注意：因为是"超集"，如果原本的查询半径本身小于判定半径（Artillery 里
/// 查询用 Radius、判定用 Radius + 1），会多出极窄一圈的候选。实测见报告。
/// </summary>
public static class RangeQueryCache
{
    /// <summary>格子边长。越大共享率越高，但单次查询覆盖范围也越大。</summary>
    public static float CellSize = 12f;

    /// <summary>缓存有效期（秒）。越大越省，代价是目标信息最多陈旧这么久。</summary>
    public static float RefreshInterval = 0.1f;

    /// <summary>累计真正执行的物理查询次数，用于验证共享率。</summary>
    public static long QueryCount;

    /// <summary>累计命中缓存（未触发物理查询）的次数。</summary>
    public static long HitCount;

    sealed class Cell
    {
        public Vector2 center;
        public float radius;
        public float expires;
        public Collider2D[] buffer = new Collider2D[128];
        public int count;
    }

    static readonly Dictionary<long, Cell> Cells = new Dictionary<long, Cell>(256);
    static readonly List<long> Scratch = new List<long>();
    static float _nextPrune;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Cells.Clear();
        Scratch.Clear();
        _nextPrune = 0f;
        QueryCount = 0;
        HitCount = 0;
    }

    static float HalfDiagonal
    {
        get { return CellSize * 0.70710678f; }
    }

    static long Key(Vector2 p, int camp)
    {
        long cx = (long)Mathf.Floor(p.x / CellSize) & 0xFFFFFF;
        long cy = (long)Mathf.Floor(p.y / CellSize) & 0xFFFFFF;
        return (cx << 40) | (cy << 16) | (uint)(camp & 0xF);
    }

    static Vector2 CellCenter(Vector2 p)
    {
        float cx = Mathf.Floor(p.x / CellSize);
        float cy = Mathf.Floor(p.y / CellSize);
        return new Vector2((cx + 0.5f) * CellSize, (cy + 0.5f) * CellSize);
    }

    /// <summary>
    /// 取覆盖「以 pos 为心、radius 为半径」这个圆的候选集。
    /// 结果写入 results（缓存内的复用数组，仅在本次调用到下一次同格查询之间有效，
    /// 必须在本帧内用完，不要跨帧持有）。
    /// 注意：缓存会保留碰撞体引用到有效期结束，期间对象可能被销毁，
    /// 遍历结果时务必先做 `if (col == null) continue;` 判空。
    /// </summary>
    public static int Get(Vector2 pos, float radius, int camp, out Collider2D[] results)
    {
        float now = Time.time;
        long key = Key(pos, camp);
        float cover = HalfDiagonal + radius;

        Cell cell;
        if (!Cells.TryGetValue(key, out cell))
        {
            cell = new Cell();
            cell.center = CellCenter(pos);
            cell.radius = cover;
            cell.expires = now + RefreshInterval;
            Cells[key] = cell;
        }
        else if (now < cell.expires)
        {
            // 未过期：本请求的圆被缓存圆覆盖就直接复用，零物理查询
            Vector2 d = pos - cell.center;
            if (d.magnitude + radius <= cell.radius)
            {
                HitCount++;
                results = cell.buffer;
                return cell.count;
            }
            // 覆盖不足（同格里有半径更大的炮塔），扩大半径重查；不延长有效期，
            // 也保证半径只增不减，避免同格内大小半径轮流请求造成反复重查
            cell.radius = cover;
        }
        else
        {
            // 过期：沿用现有半径重查并续期
            if (cover > cell.radius) cell.radius = cover;
            cell.expires = now + RefreshInterval;
        }

        cell.count = RangeQuery.OverlapCircleInto(cell.center, cell.radius, RangeQuery.TargetMask(camp), ref cell.buffer);
        QueryCount++;
        Prune(now);
        results = cell.buffer;
        return cell.count;
    }

    /// <summary>清空所有格子（切场景等场合可手动调用）。</summary>
    public static void Clear()
    {
        Cells.Clear();
    }

    static void Prune(float now)
    {
        if (now < _nextPrune) return;
        _nextPrune = now + 5f;

        Scratch.Clear();
        foreach (KeyValuePair<long, Cell> kv in Cells)
        {
            if (kv.Value.expires < now - 5f) Scratch.Add(kv.Key);
        }
        for (int i = 0; i < Scratch.Count; i++) Cells.Remove(Scratch[i]);
    }
}
