using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        List<Vector2> RL = new();
        List<int> RI = new();
        List<Vector2> BL = new();
        List<int> BI = new();
        for (int i = 0 ; i < CenterList .Count; i++)
        {
            if (CenterList[i] == null)
            {
                CenterList.RemoveAt(i);
                continue;
            }                
            if (CenterList[i].camp == 0) continue;
            if (CenterList[i].camp == 1)//right_blue
            {
                BL.Add(CenterList[i].transform.position);
                BI.Add(i);
            }
            else//left_red
            {
                RL.Add(CenterList[i].transform.position);
                RI.Add(i);
            }
        }
        int R = -1;
        Parallel.For(0, RL.Count, x =>
         {
             if (RL[x].x < 121)
             {
                 if (R == -1) R = (int)x;
                 else
                 {
                     if (RL[R].x < RL[x].x) R = (int)x;
                 }
             }
         });
        int B = -1;
        Parallel.For(0, BL.Count, x =>
        {
            if (BL[x].x > -121)
            {
                if (B == -1) B = (int)x;
                else
                {
                    if (BL[B].x > BL[x].x) B = (int)x;
                }
            }
        });

        if (R != -1) RF = CenterList[RI[R]].transform;
        if (B != -1) BF = CenterList[BI[B]].transform;
    }

    private Dictionary<Transform, Rigidbody2D> rb_cache = new();
    public float RRBV
    {
        get 
        {
            if (RF == null) return 0;
            if (!rb_cache.ContainsKey(RF)) rb_cache[RF] = RF.GetComponent<Rigidbody2D>();
            return rb_cache[RF].velocity.x;

        }
    }
    public float BRBV
    {
        get
        {
            if (BF == null) return 0;
            if (!rb_cache.ContainsKey(BF)) rb_cache[BF] = BF.GetComponent<Rigidbody2D>();
            return rb_cache[BF].velocity.x;
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
