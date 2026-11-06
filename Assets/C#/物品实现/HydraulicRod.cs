using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraulicRod : MonoBehaviour
{
    public float Offset;
    public SpriteRenderer sp;
    public BoxCollider2D bc;
    public TryConnect tc;
    RelativeJoint2D A;
    RelativeJoint2D B;

    bool first  =true;

    public int State = 0;
    public int Limit = 2;

    private void FixedUpdate()
    {

        StartCoroutine(nameof(LateFixedUpate));
    }

    IEnumerator LateFixedUpate()
    {
        yield return new WaitForFixedUpdate();
        if (first)
        {
            if (GameManager.gameState == GameManager.GameState.Start)
            {
                RelativeJoint2D[] rjl = transform.GetComponents<RelativeJoint2D>();
                for (int i = 0; i < rjl.Length; i++)
                {
                    if (rjl[i].linearOffset.y > 0)
                    {
                        A = rjl[i];
                    }
                    else
                    {
                        B = rjl[i];
                    }
                }
                for (int i = 0; i < tc.Object.Count; i++)
                {
                    if (tc.Object[i] != null)
                    {
                        foreach (RelativeJoint2D rj in tc.Object[i].GetComponents<RelativeJoint2D>())
                        {
                            if (rj.connectedBody.gameObject == gameObject)
                            {
                                if (rj.linearOffset.y > 0)
                                {
                                    A = rj;
                                }
                                else
                                {
                                    B = rj;
                                }
                            }
                        }
                    }
                }
                first = false;
            }
        }
        if (State != 0)
        {
            Offset += State * Time.deltaTime;
            Offset = Mathf.Clamp(Offset, 0, Limit);
        }
        if (last != Offset)
        {
            if (A != null)
            {
                A.linearOffset += (Offset - last) / 2f * A.linearOffset / A.linearOffset.magnitude;
            }
            if (B != null)
            {
                B.linearOffset += (Offset - last) / 2f * B.linearOffset / B.linearOffset.magnitude;
            }
            last = Offset;
        }

        float RealOffset = Offset;
        if (A != null)
        {
            RealOffset -= A.linearOffset.magnitude - (A.transform.position - A.connectedBody.transform.position).magnitude;
        }
        if (B != null)
        {
            RealOffset -= B.linearOffset.magnitude - (B.transform.position - B.connectedBody.transform.position).magnitude;
        }
        sp.size = new Vector2(1, 1 + RealOffset);
        bc.size = new Vector2(0.98f, 0.98f + RealOffset);

    }
    float last = 0;
}
