
using System.Collections.Generic;
using UnityEngine;

public class PowerWheel : MonoBehaviour, IPower, ION
{
    public bool TurnOver;
    public float MaxSpeed;//最高速度
    public float OriginalPower;//功率
    public Heath heath;
    public Rigidbody2D ownRb;
    public Rigidbody2D rb;//不是自己的事circle轮子的
    [Header("水中推进x献祭当前轮子速度的0.5f，转换为1f")]
    public Vector2 water_trans = new(0.2f, 0.2f);

    public float add_power;
    private void FixedUpdate()
    {
        if (OnSet) Work();
        Water();
    }

    float F;
    void Work()
    {
        if (rb == null) return;
        float V = Mathf.Abs(rb.angularVelocity);
        float P = OriginalPower + add_power;
        F = P * Mathf.Max(0, MaxSpeed - V) / V;
        if (F > P * 100) F = P * 100;
        if (TurnOver) F *= -1;
        if (heath.camp == 1) F *= -1;
        rb.AddTorque(F);
    }
    void Water()
    {
        if (!heath.WithEffect(Heath.Effect.Water)) return;
        float cost = Time.fixedDeltaTime * water_trans.x * rb.angularVelocity;
        rb.angularVelocity -= cost;
        ownRb.AddRelativeForce(new Vector2(-cost * water_trans.y, 0), ForceMode2D.Impulse);
    }
    public void AddPower(float set)
    {
        add_power += set * 发动机乘数;
    }
    public float 发动机乘数;

    #region 接口信息
    public bool OnSet { get; set; }

    public float NumSet { get; set; }
    public bool IsOn()
    {
        return OnSet;
    }
    #endregion
}
