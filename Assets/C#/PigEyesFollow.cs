using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigEyesFollow : MonoBehaviour
{
    public Animator anim;
    public Vector2 PigRange;
    public Vector2 YZ;
    private int mouseKeep = 0;
    private Vector3 lastPos;
    public int lookTime = 60;
    void Update()
    {
        if (lastPos == Input.mousePosition)
        {
            mouseKeep++;
        }
        else
        {
            mouseKeep = 0;
            lastPos = Input.mousePosition;
        }
        float X = 0.5f;
        float Y = 0.5f;
        if (mouseKeep <= lookTime)
        {
            Vector3 offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            offset = Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward) * offset;
            X += offset.x / (PigRange.x * transform.lossyScale.x);
            Y += offset.y / (PigRange.y * transform.lossyScale.y);
        }
        anim.Play("LookRight", -1, YZ[0] = Mathf.Clamp(Mathf.Lerp(YZ[0], X, Time.deltaTime * 5f), 0f, 0.99f));
        anim.Play("LookUp", -1, YZ[1] = Mathf.Clamp(Mathf.Lerp(YZ[1], Y, Time.deltaTime * 5f), 0f, 0.99f));
    }
}
