using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringManager : MonoBehaviour,IStart
{
    public CheckAlive ca;
    public TryConnect tc;
    public LineRenderer LR;
    public float Force;
    public float K;


    RelativeJoint2D A;//up
    RelativeJoint2D B;//down
    float A_f;
    float B_f;

    bool start;
    public void SetStart(float t)
    {
        start = true;
        for (int i = 0; i < ca.allConnect.Count; i++)
        {
            ca.allConnect[i].enabled = false;//取消之前链接 的 效果但不是删除
            RelativeJoint2D rj = gameObject.AddComponent<RelativeJoint2D>();//弹性连接
            rj.connectedBody = ca.CAList[i].GetComponent<Rigidbody2D>();
            rj.maxForce = Force;
            rj.maxTorque = Force * 1f;
            rj.correctionScale = 0.075f;
            //记录一下
            if (OwnCar.SCT(1, tc.forward) == ca.BlockOffset[i])//保存至A
            {
                A = rj;
                A_f = Vector3.Distance(transform.position, A.connectedBody.transform.position) - 0.5f;
            }
            if (OwnCar.SCT(0, tc.forward) == ca.BlockOffset[i])//保存至B
            {
                B = rj;
                B_f = Vector3.Distance(transform.position, B.connectedBody.transform.position) - 0.5f;
            }
            StartCoroutine("OffAuto", rj);
        }
    }
    IEnumerator OffAuto(RelativeJoint2D rj)
    {
        yield return new WaitForFixedUpdate();
        rj.autoConfigureOffset = false;
    }
    private void LateUpdate()
    {
        if (!start) return;
        if (LR == null) return;
        Vector3 d;
        float f;
        float ff;
        if (A != null)
        {
            f = Vector3.Distance(transform.position, A.connectedBody.transform.position);
            ff = f - A_f;
            d = Vector3.Lerp(transform.position, A.connectedBody.transform.position, ff / f);
            LR.SetPosition(0, transform.InverseTransformPoint(d));
            A.maxForce = Force + Mathf.Abs(ff) * K;
        }
        if (B != null)
        {
            f = Vector3.Distance(transform.position, B.connectedBody.transform.position);
            ff = f - B_f;
            d = Vector3.Lerp(transform.position, B.connectedBody.transform.position, ff / f);
            LR.SetPosition(2, transform.InverseTransformPoint(d));
            B.maxForce = Force + Mathf.Abs(ff) * K;
        }
    }
}
