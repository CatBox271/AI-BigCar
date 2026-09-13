using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleConnect : MonoBehaviour
{
    int Start = 4;
    private void LateUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            if (Start != 0)
            {
                if (Start > 0)
                {
                    Start--;
                }
                else
                {
                    if (rj == null || rj.connectedBody == null)
                    {
                        Destroy(LeftJoint);
                        Destroy(RightJoint);
                        Destroy(this);
                    }
                }
            }
            else
            {
                Start--;
                AddConnect();
            }
        }
    }
    RelativeJoint2D rj;
    private void AddConnect()
    {
        RelativeJoint2D[] all = GetComponents<RelativeJoint2D>();
        if (all.Length == 2)
        {
            rj = all[1];
            CheckAlive ca;
            if (rj.gameObject == gameObject)
            {
                ca = rj.connectedBody.GetComponent<CheckAlive>();
            }
            else
            {
                ca = rj.gameObject.GetComponent<CheckAlive>();
            }
            if (ca != null)
            {
                if (ca.BlockOffset.IndexOf(2) != -1)
                {
                    CheckAlive left = ca.CAList[ca.BlockOffset.IndexOf(2)];
                    LeftJoint = gameObject.AddComponent<RelativeJoint2D>();
                    LeftJoint.connectedBody = left.GetComponent<Rigidbody2D>();
                    LeftJoint.maxForce = rj.maxForce;
                    LeftJoint.maxTorque = rj.maxTorque;
                    LeftJoint.correctionScale = rj.correctionScale;
                    StartCoroutine("AutoConfireOff", LeftJoint);
                }
                if (ca.BlockOffset.IndexOf(3) != -1)
                {
                    CheckAlive right = ca.CAList[ca.BlockOffset.IndexOf(3)];
                    RightJoint = gameObject.AddComponent<RelativeJoint2D>();
                    RightJoint.connectedBody = right.GetComponent<Rigidbody2D>();
                    RightJoint.maxForce = rj.maxForce;
                    RightJoint.maxTorque = rj.maxTorque;
                    RightJoint.correctionScale = rj.correctionScale;
                    StartCoroutine("AutoConfireOff", RightJoint);
                }

            }
        }
    }

    IEnumerator AutoConfireOff(RelativeJoint2D rj)
    {
        if (rj != null)
        {
            rj.autoConfigureOffset = true;
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        if (rj != null)
        {
            rj.autoConfigureOffset = false;
        }
    }

    RelativeJoint2D LeftJoint, RightJoint;
}
