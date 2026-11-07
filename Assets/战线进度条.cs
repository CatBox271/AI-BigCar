using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 战线进度条 : MonoBehaviour
{
    public Transform Left;
    public Transform Right;

    private void Update()
    {
        Left.localScale = new Vector3((VideoCameraFollow.vcf.RRX + 120)*2f ,Left.localScale.y,Left.localScale.z);
        Right.localScale = new Vector3((120 - VideoCameraFollow.vcf.BRX) * 2f,Right.localScale.y,Right.localScale.z);
    }
}
