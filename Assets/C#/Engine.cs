using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Engine : MonoBehaviour,IStart
{
    public Transform pic;
    private void OnDestroy()
    {
        CutPower();
    }

    public float Times;

    void Update()
    {
        if (ready == -1) return;
        if (pic != null)
        {
            pic.localPosition = 0.015f * new Vector3(Mathf.Sin(Time.time * 100f), Mathf.Cos(Time.time * 100f));
        }
    }

    int lastOn = -1;
    float lastGive;
    float CF;//第一个检查
    void FixedUpdate()
    {
        if (ready == -1) return;
        if (CF == Time.time) return;
        //时刻检查Power接口
        int On = 0;

        for (int i = 0; i < AllPower.Count; i++)
        {
            if (AllPower[i] == null) continue;
            if (AllPower[i].IsOn()) On++;
        }
        if (On == lastOn) return;
        lastOn = On;
        Engine eng;
        for (int i = 0; i < SameEngine.Count; i++)//同步同载引擎
        {
            eng = SameEngine[i];
            eng.CF = CF;
            eng.lastOn = lastOn;

            eng.SetValue(-eng.lastGive);
            eng.lastGive = 0;
            if (On != 0)
            {
                eng.lastGive = eng.Times / On;
                eng.SetValue(eng.lastGive);
            }
        }
    }

    public List<IPower> AllPower = new();
    public List<Engine> SameEngine = new();

    public CheckAlive ca;
    private void SetValue(float value)
    {
        //更改目标值
        for (int i = 0; i < AllPower.Count; i++)
        {
            if (AllPower[i] == null) continue;
            AllPower[i].AddPower(value);
        }
    }

    float ready = -1;
    public void SetStart(float t)//尝试连接IPower
    {
        if (ready == t) return;
        ready = t;
        CutPower();
        AllPower.Clear();
        List<CheckAlive> allItem = new();
        allItem.AddRange(GameManager.allvehicle[ca.VID]);//对所有通一载具内容进行分析
        print(GameManager.allvehicle[ca.VID].Count + "," + ca.VID);
        SameEngine.Clear();
        Engine eng;
        for (int i = 0; i < allItem.Count; i++)
        {
            if (allItem[i] == null) continue;
            if (allItem[i].TryGetComponent(out IPower power)) AllPower.Add(power);//获得所有接口
            if (allItem[i].TryGetComponent(out eng)) SameEngine.Add(eng);
        }

        for (int i = 0; i < SameEngine.Count; i++)//同步同载引擎
        {
            eng = SameEngine[i];
            if (eng == this) continue;
            eng.SameEngine.Clear();
            eng.SameEngine.AddRange(SameEngine);
            eng.CutPower();//停止所有引擎的动力
            eng.ready = t;
            eng.AllPower.Clear();
            eng.AllPower.AddRange(AllPower);
        }
    }

    void CutPower()
    {
        SetValue(-lastGive);//停止所有引擎的动力
        lastOn = -1;
        lastGive = 0;
    }
}
