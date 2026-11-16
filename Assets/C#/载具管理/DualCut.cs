using System;
using System.Collections.Generic;

/// <summary>
/// 载具分裂判据（平面图对偶，不遍历连通分量）。
/// 删掉的连接在「面图」上成环 ⟺ 原网格被切开。
/// 面图节点 = 4 个格围成的区域（越界 = 外部面），面图的边 = 一条可以断开的连接。
/// 一次判定只看已断的那几条边，与载具大小无关。
/// </summary>
public static class DualCut
{
    /// <summary>外部面</summary>
    public const int OutFace = -1;
    /// <summary>同格连接（BlockOffset 的 4 号），不分割平面，不参与对偶</summary>
    public const int NoFace = -2;

    public interface ICutEdge
    {
        int FaceA { get; }
        int FaceB { get; }
    }

    /// <summary>面 id：左上角格 (x,y) 围成的那个区域，越界返回 OutFace。</summary>
    public static int Face(int x, int y, int w, int h)
    {
        if (x < 0 || y < 0 || x > w - 2 || y > h - 2) return OutFace;
        return y * (w - 1) + x;
    }

    /// <summary>
    /// 求一条连接两侧的面。ai / bi 是格索引，dir 是 ai→bi 的方向（0 上 1 下 2 左 3 右）。
    /// </summary>
    public static void FacesOfEdge(int ai, int bi, int dir, int w, int h, out int f1, out int f2)
    {
        int ax = ai % w, ay = ai / w;
        switch (dir)
        {
            case 0: f1 = Face(ax, ay, w, h); f2 = Face(ax - 1, ay, w, h); break;
            case 1: f1 = Face(ax, ay - 1, w, h); f2 = Face(ax - 1, ay - 1, w, h); break;
            case 2: f1 = Face(ax - 1, ay, w, h); f2 = Face(ax - 1, ay - 1, w, h); break;
            default: f1 = Face(ax, ay, w, h); f2 = Face(ax, ay - 1, w, h); break;
        }
    }

    /// <summary>断边集合在对偶图上成环 → 载具已经裂开。</summary>
    public static bool HasCycle<T>(IList<T> cuts) where T : ICutEdge
    {
        if (cuts == null || cuts.Count == 0) return false;
        Dictionary<int, int> parent = new();
        for (int i = 0; i < cuts.Count; i++)
        {
            int a = cuts[i].FaceA, b = cuts[i].FaceB;
            if (a == NoFace || b == NoFace || a == b) continue;
            int ra = Find(parent, a), rb = Find(parent, b);
            if (ra == rb) return true;
            parent[ra] = rb;
        }
        return false;
    }

    static int Find(Dictionary<int, int> parent, int x)
    {
        if (!parent.TryGetValue(x, out _)) { parent[x] = x; return x; }
        int root = x;
        while (parent[root] != root) root = parent[root];
        while (x != root) { int next = parent[x]; parent[x] = root; x = next; }
        return root;
    }

    /// <summary>
    /// 从 start 出发淋水，跳过已经断掉的连接，收集这一侧的全部格子。
    /// 调用方自己判大小：要的是小侧就取一侧，不分大小就换另一侧。
    /// </summary>
    public static void CollectSide<T>(T start, Func<T, IList<T>> neighbors,
        Func<T, T, bool> isCut, HashSet<T> visited, Queue<T> queue, List<T> result) where T : class
    {
        result.Clear();
        visited.Clear();
        queue.Clear();
        if (start == null) return;
        visited.Add(start);
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            T cur = queue.Dequeue();
            result.Add(cur);
            IList<T> neighborsOfCur = neighbors(cur);
            if (neighborsOfCur == null) continue;
            for (int i = 0; i < neighborsOfCur.Count; i++)
            {
                T next = neighborsOfCur[i];
                if (next == null) continue;
                if (visited.Contains(next)) continue;
                if (isCut(cur, next)) continue;
                visited.Add(next);
                queue.Enqueue(next);
            }
        }
    }
}
