using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CheckAlive))]
public class BlockJoint2D : MonoBehaviour,IStart
{
    public bool EnableCollison = false;
    public float correctScale = 15f;
    Rigidbody2D ownRB;
    public Rigidbody2D connectedBody;
    public float maxForce = 100000f;
    public float maxTorque = 100000f;
    public float breakForce = 100f;
    public float breakTorque = 100f;
    public Vector2 Offset = new();
    public float AngleOffset = new();
    CheckAlive ca;

    public List<BlockJoint2D> allJoint = new();//只能连上能连上的，但为了线验证效果展示不做
    public List<Rigidbody2D> allBody = new();//所有和BJ有链接的RB
    public List<Vector2> OS = new(); //与allBody一二对应,AimShape的坐标是所有BJ的平均中心
    public float AA;//目标旋转
    public List<Vector2> AS = new();//目标形状

    float ready;
    public void SetStart(float t)
    {
        if (ready == t) return;
        ready = t;
        allJoint.Clear();
        allBody.Clear();
        List<CheckAlive> allItem = new();
        allItem.AddRange(GameManager.allvehicle[ca.VID]);//对所有通一载具内容进行分析
        BlockJoint2D bj;
        for (int i = 0; i < allItem.Count; i++)
        {
            if (allItem[i] == null) continue;
            allJoint.AddRange(allItem[i].GetComponents<BlockJoint2D>());//获得所有BlockJoint2D
        }
        //然后对所有bj分析
        Vector2 v2;
        //先算ShapeAngle
      
        for (int i = 0; i < allJoint.Count; i++)
        {
            bj = allJoint[i];
            Transform tf = bj.transform;
            //同时遍历获取所有Rigidbody
            TryAddBody(bj.ownRB);
            TryAddBody(bj.connectedBody);
        }
        //再算每个Body的Rigidbody
        v2 = new();//重置
        float MassAll = 0;
        for (int i = 0; i < allBody.Count; i++)
        {
            Rigidbody2D rb = allBody[i];
            v2 += rb.position * rb.mass;
            MassAll += rb.mass;
        }
        v2 /= MassAll;//求平均
        OS.Clear();
        //现在得到了中心位置,接下来再遍历一次，得到和中心的相对坐标
        for (int i = 0; i < allBody.Count; i++)
        {
            Vector2 r = allBody[i].position - v2;
            OS.Add(r);
        }
        //记得补上虚拟链接分组的情况
        for (int i = 0; i < allJoint.Count; i++)//同步同载连接
        {
            SetSame(allJoint[i],Time.time);
        }
    }
    void SetSame(BlockJoint2D bj,float t)
    {
        if (bj == null) return;
        if (bj == this) return;
        bj.allJoint.Clear();
        bj.allJoint.AddRange(allJoint);
        bj.OS.Clear();
        bj.OS.AddRange(OS);
        bj.allBody.Clear();
        bj.allBody.AddRange(allBody);
        SetAim(bj, t);
    }
    void SetAim(BlockJoint2D bj, float t)
    {
        bj.AA = AA;
        bj.AS.Clear();
        bj.AS.AddRange(AS);
        bj.ready = t;
    }
    /// <summary>
     /// 将角度限制在+-360
     /// </summary>
     /// <param name="current">输入</param>
     /// <returns></returns>
    float ToSignedAngle(float current)
    {
        bool negtive = current < 0;
        current = Mathf.Abs(current);
        return current % 360 * (negtive ? -1 : 1);
    }
    float CircleAngle(float a)
    {
        if (a < 0)
        {
            a += 360f;
        }
        return a;
    }
    private void FixedUpdate()
    {
        if (ready != Time.time)//计算整体框架目标值
        {
            BlockJoint2D bj;
            Vector2 v2;
            Vector2 all_v2 = new();
            //先算ShapeAngle

            for (int i = 0; i < allJoint.Count; i++)
            {
                bj = allJoint[i];
                Transform tf = bj.transform;

            }

            AA = 0;//重置
            float totalAngle = 0;
            //再算每个Body的Rigidbody
            v2 = new();//重置
            List<Vector2> EachMV = new();
            Vector2 motion = new();//动量
            float angle_motion = new(); //角动量
            float totalInertia = 0;
            float MassAll = 0;
            for (int i = 0; i < allBody.Count; i++)
            {
                Rigidbody2D rb = allBody[i];
                v2 += rb.position * rb.mass;
                MassAll += rb.mass;
                EachMV.Add(rb.velocity * rb.mass);
            }
            v2 /= MassAll;//求平均
            float vl = 0;
            for (int i = 0; i < EachMV.Count; i++)
            {
                Rigidbody2D rb = allBody[i];
                Vector2 mv = EachMV[i];
                motion += mv;
                Vector2 r = rb.position - v2;
                angle_motion += r.x * mv.y - r.y * mv.x;
                totalInertia += rb.inertia + rb.mass * r.sqrMagnitude;
                totalAngle += Vector2.SignedAngle(OS[i], r) * rb.mass;
            }
            totalAngle /= MassAll;
            motion /= MassAll;
            angle_motion /= totalInertia / Mathf.Rad2Deg;
            AA = totalAngle;
            AS.Clear();
            float dt = Time.fixedDeltaTime;
            for (int i = 0; i < OS.Count; i++)
            {
                AS.Add((Vector2)(Quaternion.AngleAxis(-totalAngle - angle_motion * dt, -Vector3.forward) * OS[i]) + v2 + motion * dt);//世界坐标位置
            }

            //最后将进行物理计算

            for (int i = 0; i < allBody.Count; i++)
            {
                PhysicsSimulate(allBody[i], i);
            }

            allJoint.ForEach(joint => joint.ready = Time.time);
        }
    }

    void PhysicsSimulate(Rigidbody2D rb ,int i)
    {
        float dt = Time.fixedDeltaTime;
        Vector2 FA = AS[i] - (rb.position + rb.velocity * dt);

        FA = FA / dt * rb.mass * correctScale;

        rb.AddForce(FA);
    } 
    void TryAddBody(Rigidbody2D rb)
    {
        if (rb == null) return;
        if (allBody.Contains(rb)) return;
        allBody.Add(rb);
    }
    public void AutoConfigure()
    {
        TryGetComponent(out ownRB);
        TryGetComponent(out ca);
        if (connectedBody == null) return;

        if (!EnableCollison || true) Physics2D.IgnoreCollision(ownRB.GetComponent<Collider2D>(), connectedBody.GetComponent<Collider2D>());

        Offset = transform.InverseTransformPoint(connectedBody.position);//基于本地坐标系//受缩放影响
        AngleOffset = connectedBody.transform.localEulerAngles.z - transform.localEulerAngles.z;
    }
}
