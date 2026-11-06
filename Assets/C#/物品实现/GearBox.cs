using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearBox : MonoBehaviour
{
    public bool On = true;
    public int Gear = 1;
    public GameObject joystick;

    public GameObject G1;
    public GameObject G2;
    public GameObject G3;
    public GameObject G4;
    public GameObject G5;
    public GameObject G6;

    int Part = 0;
    float t;
    void Update()
    {
        Vector3 Speed = new Vector3(0, 0, 60) * Gear * Time.deltaTime;
        G1.transform.localEulerAngles += Speed * 2;
        G2.transform.localEulerAngles += Speed * 2;
        G3.transform.localEulerAngles += Speed * 2;
        G4.transform.localEulerAngles += Speed;
        G5.transform.localEulerAngles += Speed;
        G6.transform.localEulerAngles += Speed;
        switch (Gear)
        {
            case -1:
                ChangeGear(-1, -1);
                break;
            case 0:
                ChangeGear(0, 0);
                break;
            case 1:
                ChangeGear(-1, 1);
                break;
            case 2:
                ChangeGear(0, -1);
                break;
            case 3:
                ChangeGear(0, 1);
                break;
            case 4:
                ChangeGear(1, -1);
                break;
            case 5:
                ChangeGear(1, 1);
                break;
        }

    }

    private void FixedUpdate()
    {
       
    }
    int LastGear;
    void ChangeGear(float X,float Y)
    {
        if (LastGear != Gear)
        {
            LastGear = Gear;
            Part = -2;
            t = 1.114514f;
        }
        if (Part == 0)
        {
            if (t == 1.114514f)
            {
                t = 0;
            }
            joystick.transform.localPosition = new Vector3(Mathf.Lerp(joystick.transform.localPosition.x, 0.228f * X, t), 0);
            t += Time.deltaTime * 5f;
            if (joystick.transform.localPosition.x == 0.228f * X)
            {
                t = 1.114514f;
                Part = 1;
            }
        }
        if (Part == 1)
        {
            if (t == 1.114514f)
            {
                t = 0;
            }
            joystick.transform.localPosition = new Vector3(joystick.transform.localPosition.x, Mathf.Lerp(joystick.transform.localPosition.y,  0.15f * Y, t));
            t += Time.deltaTime * 5f;
            if (joystick.transform.localPosition.y == 0.15f * Y)
            {
                t = 1.114514f;
            }
        }
        if (Part == -2)
        {
            if (t == 1.114514f)
            {
                t = 0;
            }
            joystick.transform.localPosition = new Vector3(joystick.transform.localPosition.x, Mathf.Lerp(joystick.transform.localPosition.y, 0, t));
            t += Time.deltaTime * 5f;
            if (joystick.transform.localPosition.y ==0)
            {
                t = 1.114514f;
                Part = -1;
            }
        }
        if (Part == -1)
        {
            if (t == 1.114514f)
            {
                t = 0;
            }
            joystick.transform.localPosition = new Vector3(Mathf.Lerp(joystick.transform.localPosition.x, 0, t), 0);
            t += Time.deltaTime * 5f;
            if (joystick.transform.localPosition.x == 0)
            {
                t = 1.114514f;
                Part = 0;
            }
        }
    }

    private void OnDestroy()
    {
        SetValue(0);
    }

    public float Times;

    public List<Transform> AllBlock = new();

    int first = 4;

    private void Awake()
    {
        first += Random.Range(0, 3);
    }
   
    

    private void SetValue(int value)
    {
        //更改目标值
        for (int i = 0; i < AllBlock.Count; i++)
        {
            Transform a = AllBlock[i];
            if (a != null)
            {
                //轮子
                if (a.parent.childCount >= 2)
                {
                    if (a.parent.GetChild(1).TryGetComponent(out PowerWheel w))
                    {
                        //w.Gear = value;
                    }
                }
            }
        }
    }
}
