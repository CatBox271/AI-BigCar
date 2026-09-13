using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTech : MonoBehaviour
{
    public OwnCar oc;
    public Text CarNum;
    public Text CarHP;
    public Text CarSpeed;
    public Text CarHit;
    public Text CarRadius;
    public bool Red;

    private void Update()
    {
        CarNum.text = oc.CarNum + "";
        if (Red)
        {
            CarHP.text = "生命加成：" + oc.HeathUP;
            CarSpeed.text = "速度加成：" + oc.SpeedUP;
            CarHit.text = "攻击加成：" + oc.HitUP;
            CarRadius.text = "射程加成：" + oc.RangeUP;
        }
        else
        {
            CarHP.text = oc.HeathUP + "：生命加成";
            CarSpeed.text = oc.SpeedUP + "：速度加成";
            CarHit.text = oc.HitUP + "：攻击加成";
            CarRadius.text = oc.RangeUP+ "：射程加成";
        }
    }
}
