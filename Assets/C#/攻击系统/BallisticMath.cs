using UnityEngine;

/// <summary>
/// 弹道拦截求解：给定目标的相对位置与速度、弹速与重力，求出最早命中的飞行时间与发射角。
///
/// 数学形式：设相对初位置 P=(px,py)、相对速度 Vr=(vrx,vry)，令 u=speed·cosθ、w=speed·sinθ。
/// 由「飞行 T 后弹丸与目标位移相等」：
///     T(u − vrx) = px
///     T(w − vry) = py + ½gT²
///     u² + w² = speed²
/// 把前两式代入第三式并乘 T²，得到四次方程
///     ¼g²T⁴ + vry·g·T³ + (vrx²+vry²−speed²+py·g)T² + 2(px·vrx+py·vry)T + (px²+py²) = 0
/// 本类求它的最小正根（即最早拦截，对应低伸弹道）。
///
///   gravity == 0：方程退化为二次，闭式一次解出（机器精度）。
///   gravity != 0：对残差 f(T) = (px/T+vrx)² + (py/T+vry+½gT)² − speed² 先在几何扩张/收缩下
///                 定位包围区间（f(lo)&gt;0、f(hi)≤0），再用二分 + 安全 Newton 收敛到最小正根。
///                 每一步只有十余次浮点运算、不含超越函数，且有包围保证必然收敛——
///                 不像定点迭代那样收敛速度取决于 |Vr|/speed。
///
/// 发射角用 atan2(w, u) 直接给出，不用 acos，因此不存在越界产生 NaN 的问题，
/// 也不需要 acos 之后的符号修正分支。
/// </summary>
public static class BallisticMath
{
    const int MaxBracketSteps = 60;     // 包围区间搜索步数上限
    const int MaxSolveSteps = 80;       // 二分/Newton 迭代上限
    const float RelTolerance = 1e-6f;   // 相对收敛容差

    /// <summary>
    /// 求最早命中所需的飞行时间与发射角（度）。返回 false 表示按当前相对位置/速度够不到。
    /// </summary>
    public static bool Solve(float px, float py, float vrx, float vry, float speed, float gravity,
                             out float time, out float angleDeg)
    {
        time = 0f;
        angleDeg = 0f;

        if (speed <= 0f) return false;
        if (px == 0f && py == 0f) return false;     // 与目标重合，无唯一解

        if (gravity == 0f) return SolveNoGravity(px, py, vrx, vry, speed, out time, out angleDeg);
        return SolveWithGravity(px, py, vrx, vry, speed, gravity, out time, out angleDeg);
    }

    // ------------------------------------------------------------------
    // 无重力：直线拦截，方程退化为二次
    //   (vrx²+vry²−speed²)T² + 2(px·vrx+py·vry)T + (px²+py²) = 0
    // ------------------------------------------------------------------
    static bool SolveNoGravity(float px, float py, float vrx, float vry, float speed,
                               out float time, out float angleDeg)
    {
        time = 0f;
        angleDeg = 0f;

        float a = vrx * vrx + vry * vry - speed * speed;
        float b = 2f * (px * vrx + py * vry);
        float c = px * px + py * py;

        float T;
        if (Mathf.Abs(a) < 1e-6f * speed * speed)
        {
            // 目标相对速度与弹速等大，退化成一次方程
            if (Mathf.Abs(b) < 1e-9f) return false;
            T = -c / b;
            if (T <= 0f) return false;
        }
        else
        {
            float disc = b * b - 4f * a * c;
            if (disc < 0f) return false;
            float sq = Mathf.Sqrt(disc);
            float t1 = (-b + sq) / (2f * a);
            float t2 = (-b - sq) / (2f * a);
            if (t1 > t2) { float sw = t1; t1 = t2; t2 = sw; }
            T = t1 > 0f ? t1 : t2;
            if (T <= 0f || float.IsNaN(T)) return false;
        }

        float u = px / T + vrx;
        float w = py / T + vry;
        time = T;
        angleDeg = Mathf.Atan2(w, u) * Mathf.Rad2Deg;
        return true;
    }

    // ------------------------------------------------------------------
    // 有重力：包围区间 + 二分/安全 Newton
    // ------------------------------------------------------------------
    static bool SolveWithGravity(float px, float py, float vrx, float vry, float speed, float gravity,
                                 out float time, out float angleDeg)
    {
        time = 0f;
        angleDeg = 0f;

        // 搜索区间。有重力时弹丸在时刻 T 的可达集是「以自由落体点为圆心、半径 speed·T 的圆」，
        // 判据就是残差本身 f(T) ≤ 0，并不存在更紧的二次式必要条件（那是无重力时才有的）。
        // 因此这里只给一个足够宽松的时间上界，再由双向搜索覆盖起点两侧，避免把真正的解切掉。
        float c = px * px + py * py;
        float scale = Mathf.Sqrt(c) / speed;
        if (scale < 1e-4f) scale = 1e-4f;
        float tMax = 100f * (scale + speed / gravity + 1f);
        if (tMax > 1e5f) tMax = 1e5f;

        // 找包围区间：lo 处 f>0、hi 处 f≤0，且最小正根落在 [lo, hi]。
        float lo = -1f;
        float hi = -1f;
        float Tscan;

        if (Residual(scale, px, py, vrx, vry, speed, gravity) <= 0f)
        {
            // 起点已在可行区内，向下走到它的下边界
            hi = scale;
            Tscan = scale;
            for (int i = 0; i < MaxBracketSteps; i++)
            {
                float nx = Tscan / 1.5f;
                if (nx < 1e-5f) break;
                if (Residual(nx, px, py, vrx, vry, speed, gravity) <= 0f) hi = nx;
                else { lo = nx; break; }
                Tscan = nx;
            }
        }
        else
        {
            // 先向上找（弹道解通常比直线飞行时间更长）
            Tscan = scale;
            for (int i = 0; i < MaxBracketSteps; i++)
            {
                float nx = Tscan * 1.5f;
                if (nx > tMax) break;
                if (Residual(nx, px, py, vrx, vry, speed, gravity) <= 0f) { lo = Tscan; hi = nx; break; }
                Tscan = nx;
            }
            // 向上没找到，说明可行区在下方：从起点向下穿过它，从下边界穿出时得到 lo
            if (hi < 0f)
            {
                Tscan = scale;
                bool entered = false;
                for (int i = 0; i < MaxBracketSteps; i++)
                {
                    float nx = Tscan / 1.5f;
                    if (nx < 1e-5f) break;
                    if (Residual(nx, px, py, vrx, vry, speed, gravity) <= 0f) { hi = nx; entered = true; }
                    else if (entered) { lo = nx; break; }
                    Tscan = nx;
                }
            }
        }
        if (lo < 0f || hi < 0f) return false;

        for (int i = 0; i < MaxSolveSteps; i++)
        {
            if (hi - lo <= RelTolerance * Mathf.Max(1f, hi)) break;

            float mid = 0.5f * (lo + hi);
            float u = px / mid + vrx;
            float w = py / mid + vry + 0.5f * gravity * mid;
            float fm = u * u + w * w - speed * speed;

            float step = mid;
            float du = -px / (mid * mid);
            float dw = -py / (mid * mid) + 0.5f * gravity;
            float df = 2f * u * du + 2f * w * dw;
            if (df != 0f)
            {
                float cand = mid - fm / df;
                if (cand > lo && cand < hi && !float.IsNaN(cand) && !float.IsInfinity(cand)) step = cand;
            }

            float fs = (step == mid) ? fm : Residual(step, px, py, vrx, vry, speed, gravity);
            if (fs > 0f) lo = step;
            else hi = step;
        }

        float T = hi;
        float uu = px / T + vrx;
        float ww = py / T + vry + 0.5f * gravity * T;
        float sp = Mathf.Sqrt(uu * uu + ww * ww);

        // 解出的初速必须等于弹速，否则说明没有收敛到真正的根
        if (!(sp > 0f) || Mathf.Abs(sp - speed) > 0.01f * speed) return false;

        time = T;
        angleDeg = Mathf.Atan2(ww, uu) * Mathf.Rad2Deg;
        return true;
    }

    static float Residual(float T, float px, float py, float vrx, float vry, float speed, float gravity)
    {
        float u = px / T + vrx;
        float w = py / T + vry + 0.5f * gravity * T;
        return u * u + w * w - speed * speed;
    }
}
