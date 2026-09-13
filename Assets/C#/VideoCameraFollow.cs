using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VideoCameraFollow : MonoBehaviour
{
    public static List<Heath> CenterList = new();
    public static VideoCameraFollow vcf;
    private void Awake()
    {
        state = -100000000;
        CenterList = new();
        vcf = this;
    }
    public Transform RF;//RedFirst
    public Transform BF;//BlueFirst
    public static float state = -100000000;

    private void Update()
    {
        Calculate();
        Move();
    }

    public float AimX = 0;
    public float IX = 0;
    public float V;
    Vector3 T;

    bool combined = false;
    void Move()
    {
        if (state == -100000000) return;
        if (!combined)
        {
            combined = true;
            transform.position = new Vector3(state, transform.position.y, transform.position.z);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        float R = 0; float B = 0;
        if (RF != null) R = RF.position.x - -110;
        if (BF != null) B = 110 - BF.position.x;
        if (R > B)
        {
            AimX = (R - 110);
        }
        else
        {
            AimX = (110 - B);
        }
        T = transform.position;
        IX = Mathf.Lerp(IX, AimX, V);
        T.x = Mathf.Lerp(T.x, IX, V);
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
}
