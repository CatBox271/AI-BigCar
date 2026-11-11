using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让刚体自动回正到目标角度：线性弹簧 + 阻尼。
/// 只改角速度（= 给刚体加角动量，ΔL = I·Δω），不直接写 rotation，
/// 所以回正过程照常参与碰撞与关节求解。
///
/// 角加速度 = stiffness × 角度误差(度) − damping × 角速度(度/秒)
/// 临界阻尼 = 2 × √stiffness；小于它 = 欠阻尼（会过冲一下再收敛），大于它 = 过阻尼（慢但不冲过头）。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AngleUP : MonoBehaviour
{
    [Tooltip("目标角度（度）。0 = 回正到竖直（绕 Z 轴）")]
    public float targetAngle = 0f;

    [Tooltip("弹簧刚度 k（1/秒²）：角度差 1 度产生 k 度/秒² 的角加速度。越大回正越快")]
    public float stiffness = 20f;

    [Tooltip("阻尼 c（1/秒）：角加速度 -= c × 当前角速度。临界阻尼 = 2×√stiffness")]
    public float damping = 4f;

    [Tooltip("角加速度上限（度/秒²）。0 = 不限制")]
    public float maxAccel = 0f;

    [Tooltip("角度误差阈值（度）：误差小于等于它、且角速度也小于 settledSpeed 时不再施加")]
    public float deadZone = 0.2f;

    [Tooltip("角速度阈值（度/秒），配合 deadZone 判定已回正，让刚体正常休眠")]
    public float settledSpeed = 1f;

    [Tooltip("超出判定范围时唤醒刚体（休眠中的刚体不会被角速度推动）")]
    public bool wakeUp = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // DeltaAngle 走最短路径，结果落在 (-180, 180]
        float err = Mathf.DeltaAngle(rb.rotation, targetAngle);
        float w = rb.angularVelocity;

        // 已经回正且基本停住：不施加，交给刚体自己休眠
        if (Mathf.Abs(err) <= deadZone && Mathf.Abs(w) <= settledSpeed) return;

        if (wakeUp && !rb.IsAwake()) rb.WakeUp();

        float accel = stiffness * err - damping * w;
        if (maxAccel > 0f) accel = Mathf.Clamp(accel, -maxAccel, maxAccel);

        // 加角动量：Δω = 角加速度 × dt，之后由引擎继续积分
        rb.angularVelocity = w + accel * Time.fixedDeltaTime;
    }
}
