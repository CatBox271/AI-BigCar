using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoCameraFollow : MonoBehaviour
{
    [Header("相机地图半宽")]
    public float cameraW = 0;

    public static List<Heath> CenterList = new();
    public static VideoCameraFollow vcf;
    public float Velaffect = 0.75f;
    private void Awake()
    {
        CenterList = new();
        vcf = this;
    }
    public Transform RF;//RedFirst
    public Transform BF;//BlueFirst

    private void Update()
    {
        Calculate();
        Move();
    }

    public float AimX = 0;
    public float IX = 0;
    public float V;
    Vector3 T;
    void Move()
    {
        float R = -110; float B = 110;
        if (RF != null) R = RF.position.x;
        if (BF != null) B = BF.position.x;
        float VEl = 0;
        if (R > B)
        {
            //交叉
            if (R + B > 0)
            {
                AimX = R;
                VEl = RRBV;
            }
            else
            {
                AimX = B;
                VEl = BRBV;
            }
        }
        else
        {
            AimX = (R + B) / 2f;
        }
        T = transform.position;
        IX = Mathf.Lerp(IX, AimX, V);
        T.x = Mathf.Lerp(T.x, IX + Mathf.Lerp(0, VEl, Velaffect), V);

        transform.position = T;
    }
    void Calculate()
    {
        Transform red = null;
        Transform blue = null;
        for (int i = CenterList.Count - 1; i > -1; i--)//倒序遍历，RemoveAt 不会漏掉后一个
        {
            Heath h = CenterList[i];
            if (h == null)
            {
                CenterList.RemoveAt(i);
                continue;
            }
            if (h.camp == 0) continue;
            Transform t = h.transform;
            if (h.camp == 1)//right_blue，取最靠左的
            {
                if (t.position.x <= -121f) continue;
                if (blue == null || t.position.x < blue.position.x) blue = t;
            }
            else//left_red，取最靠右的
            {
                if (t.position.x >= 121f) continue;
                if (red == null || t.position.x > red.position.x) red = t;
            }
        }
        if (red != null && red != RF)
        {
            RF = red;
            rrb = null;
        }
        if (blue != null && blue != BF)
        {
            BF = blue;
            brb = null;
        }
    }

    Rigidbody2D rrb;
    Rigidbody2D brb;
    public float RRBV
    {
        get 
        {
            if (RF == null) return 0;
            if (rrb == null) rrb = RF.GetComponent<Rigidbody2D>();
            return rrb.velocity.x;

        }
    }
    public float BRBV
    {
        get
        {
            if (BF == null) return 0;
            if (brb == null) brb = BF.GetComponent<Rigidbody2D>();
            return brb.velocity.x;
        }
    }
    public float RRX
    {
        get
        {
            float R = -110;
            if (RF != null) R = RF.position.x;
            return R;
        }
    }
    public float RX
    {
        get
        {
            float R = 0;
            if (RF != null) R = 110 + RF.position.x;
            return Mathf.Min(R - 110, MX - cameraW);
        }
    }
    public float BRX
    {
        get
        {
            float B = 110;
            if (BF != null) B = BF.position.x;
            return B;
        }
    }
    public float BX
    {
        get
        {
            float B = 0;
            if (BF != null) B = 110 - BF.position.x;
            return Mathf.Max(110 - B, MX + cameraW);
        }
    }

    public float MX
    {
        get 
        {
            return transform.position.x;
        }
    }
}
