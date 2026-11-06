using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int state;

    public List<Battery> AllBattery = new();
    public List<Engine> AllEngine = new();
    public List<CheckAlive> AllBlock = new();

    public Transform Ro;
    public GameObject EN;
    public GameObject GE;
    public void Search()
    {
        if (AllBattery.Count == 0)
        {
            GetComponent<CheckAlive>().Search(new List<string>() { "Battery"}, true, new()).ForEach(s => AllBattery.Add(s.GetComponent<Battery>()));
            GetComponent<CheckAlive>().Search(new List<string>() { "Engine" }, true, new()).ForEach(s => AllEngine.Add(s.GetComponent<Engine>()));
            GetComponent<CheckAlive>().Search(new List<string>() { "Power" }, true, new()).ForEach(s => AllBlock.Add(s));
        }
    }

    int first = 4;

    private void Awake()
    {
        first += Random.Range(0, 3);
    }
    private void FixedUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            first--;
            if (first == 0)
            {
                Search();
            }

            if (state == 1)
            {
                if (lastState != 1)
                {
                    SetBlock(Times, Times);
                    lastOn = 0;
                    lastTimes = Times;
                    EN.SetActive(true);
                    GE.SetActive(false);
                }
                EngineMode();

            }
            else
            {
                if (lastState == 1)
                {
                    SetBlock(-lastGive, -lastTimes);
                    EN.SetActive(false);
                    GE.SetActive(true);
                }
            }
            if (state == -1)
            {
                GeneratorMode();
            }
            lastState = state;
        }
    }
    int lastState = 0;
    public float Times;

    public float GetEngine;
    public void GeneratorMode()
    {
        float ft = Time.fixedDeltaTime;
        float more = GetEngine * ft / 13.5f + 5f * Time.fixedDeltaTime;
        Ro.localEulerAngles -= new Vector3(0, 0, more * 30f);
        foreach (Battery ba in AllBattery)
        {
            if (ba == null) continue;
            float left = ba.MaxValue - ba.value;
            if (left > 0)
            {
                if (more > left)
                {
                    more -= left;
                    ba.value += left;
                }
                else
                {
                    ba.value += more;
                    more = 0;
                }
            }
            if (more == 0)
            {
                break;
            }
        }
    }

    public Transform EnginePic;
    public void EngineMode()
    {
        if (EnginePic != null)
        {
            EnginePic.localPosition = 0.005f * new Vector3(Mathf.Sin(Time.time * 200f), Mathf.Cos(Time.time * 200f));
        }

        float RelativeTimes = 0;
        float TF = Time.fixedDeltaTime * Times / 13.5f;
        Ro.localEulerAngles += new Vector3(0, 0, TF * 30f);
        foreach (Battery ba in AllBattery)
        {
            if (ba.value >= TF)
            {
                ba.value -= TF;
                RelativeTimes += TF / Time.fixedDeltaTime * 13.5f;
                TF = 0;
            }
            else
            {
                RelativeTimes += ba.value / Time.fixedDeltaTime * 13.5f;
                TF -= ba.value;
                ba.value = 0;
            }
            if (TF == 0)
            {
                break;
            }
        }
        int On = 0;
        for (int i = 0; i < AllBlock.Count; i++)
        {
            Transform a = AllBlock[i].transform;
            if (a != null)
            {
                //轮子
                if (a.name.Contains("Wheel"))
                {
                    if (a.parent.childCount >= 2)
                    {
                        if (a.parent.GetChild(1) != null)
                        {
                            if (a.parent.GetChild(1).TryGetComponent(out PowerWheel w))
                            {
                                if (w.OnSet) On++;
                            }
                        }
                    }
                }
                if (a.name.Contains("Umbrella"))
                {
                    if (a.TryGetComponent(out PowerUmbrella u))
                    {
                        if (u.On)
                        {
                            On++;
                        }
                    }
                }
                //其他
                if (a.name.Contains("Propeller"))
                {
                    if (a.TryGetComponent(out Propellers p))
                    {
                        if (p.OnSet)
                        {
                            On++;
                        }
                    }
                }
                if (a.name.Contains("Fan"))
                {
                    if (a.TryGetComponent(out Fan f))
                    {
                        if (f.On)
                        {
                            On++;
                        }
                    }
                }
            }
        }

        if (On != 0 && (lastTimes !=  RelativeTimes|| On != lastOn))
        {
            SetBlock(-lastGive, -lastTimes);
            SetBlock(RelativeTimes / On, RelativeTimes);
            lastTimes = RelativeTimes;
            lastOn = On;
        }
    }

    float lastOn = 0;
    float lastTimes = 0;
    private void OnDestroy()
    {
        SetBlock(-lastGive, -Times);
    }

    float lastGive = 0;
    
    private void SetBlock(float value, float maxspeed)
    {
        lastGive = value;
        List<Engine> SameEngine = new();
        //更改目标值
        for (int i = 0; i < AllBlock.Count; i++)
        {
            Transform a = AllBlock[i].transform;
            if (a != null)
            {
                //轮子
                if (a.name.Contains("Wheel"))
                {
                    if (a.parent.childCount >= 2)
                    {
                        if (a.parent.GetChild(1) != null)
                        {
                            if (a.parent.GetChild(1).TryGetComponent(out PowerWheel w))
                            {
                                w.MaxSpeed += maxspeed * 20f;
                            }
                        }
                    }
                }
                if (a.name.Contains("Umbrella"))
                {
                    if (a.TryGetComponent(out PowerUmbrella u))
                    {
                        u.Speed += value / 27f * 1.5f;
                    }
                }
                //其他
                if (a.name.Contains("Propeller"))
                {
                    if (a.name.Contains("Water"))
                    {
                        if (a.name.Contains("Small"))
                        {
                            if (a.TryGetComponent(out Propellers p))
                            {
                                p.Force -= value / 60f;
                                p.MaxSpeed += maxspeed / 1200f;
                            }
                        }
                        else
                        {
                            if (a.TryGetComponent(out Propellers p))
                            {
                                p.Force -= value / 30f;
                                p.MaxSpeed += maxspeed / 1000f;
                            }
                        }
                    }
                    else
                    {
                        if (a.name.Contains("Small"))
                        {
                            if (a.TryGetComponent(out Propellers p))
                            {
                                p.Force += value / 5f;
                                p.MaxSpeed += maxspeed / 20f;
                            }
                        }
                        else
                        {
                            if (a.TryGetComponent(out Propellers p))
                            {
                                p.Force += value / 1.5f;
                                p.MaxSpeed += maxspeed / 100f;
                            }
                        }
                    }
                }
                if (a.name.Contains("Fan"))
                {
                    if (a.TryGetComponent(out Fan f))
                    {
                        f.Force += value / 7.5f;
                        f.MaxSpeed += maxspeed / 50f;
                    }
                }
            }
        }
        for (int i = 0; i < SameEngine.Count; i++)
        {
            if (SameEngine[i].AllPower.Count == 0)
            {

            }
        }
    }

}
