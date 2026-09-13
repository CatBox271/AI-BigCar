using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stealth : MonoBehaviour
{
    public bool State;
    public float PowerConsumtion;
    private void OnDestroy()
    {
        SetValue(false);
    }
    public float Times;

    public List<Transform> AllBlock = new();
    public List<Battery> AllBattery = new();
    public int first = 4;

    bool Video;
    private void Awake()
    {
        first += Random.Range(0, 3);
        Video = Camera.main.name == "VideoMode";
        if (Video) first = -1;
    }
    public float SetAddTime = 20f;
    float AddValue = 0;
    void FixedUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            first--;
            if (first == 0)
            {
                Search();
                if (Video)
                {
                    State = true;
                    SetValue(true);
                }
            }
            if (Video)
            {
                if (!State)
                {
                    if (lastState != State)
                    {
                        SetValue(false);
                    }
                }
            }
            else
            {
                if (State)
                {
                    if (lastState != State)
                    {
                        AddValue = SetAddTime;
                    }
                    float AllConsumption = AllBlock.Count * PowerConsumtion * Time.fixedDeltaTime;
                    float AllValue = AddValue;
                    for (int i = 0; i < AllBattery.Count && AllValue < AllConsumption; i++)
                    {
                        AllValue += AllBattery[i].value;
                    }
                    if (AllValue >= AllConsumption)
                    {
                        if (AddValue != 0)
                        {
                            if (AddValue > AllConsumption)
                            {
                                AddValue -= AllConsumption;
                                AllConsumption = 0;
                            }
                            else
                            {
                                AllConsumption -= AddValue;
                                AddValue = 0;
                            }
                        }

                        for (int i = 0; i < AllBattery.Count && AllConsumption != 0; i++)
                        {
                            if (AllBattery[i].value >= AllConsumption)
                            {
                                AllBattery[i].value -= AllConsumption;
                                AllConsumption = 0;
                            }
                            else
                            {
                                AllConsumption -= AllBattery[i].value;
                                AllBattery[i].value = 0;
                            }
                        }

                        if (lastState != State)
                        {
                            SetValue(true);
                        }
                    }
                    else
                    {
                        State = false;
                        SetValue(false);
                    }
                }
                else
                {
                    if (lastState != State)
                    {
                        SetValue(false);
                    }
                }
                lastState = State;
            }
        }
    }

    bool lastState;
    public void Search()
    {
        AllBlock = new() { transform };
        GetComponent<CheckAlive>().Search(new List<string>() { "" }, true, new()).ForEach(s => AllBlock.Add(s.transform));
        GetComponent<CheckAlive>().Search(new List<string>() { "Battery" }, true, new()).ForEach(s => AllBattery.Add(s.GetComponent<Battery>()));
    }
    public void SetValue(bool value)
    {
        //更改目标值
        for (int i = 0; i < AllBlock.Count; i++)
        {
            Transform a = AllBlock[i];
            if (a != null)
            {
                if (a.TryGetComponent(out Heath _))
                {
                    if (value)
                    {
                        if (!a.TryGetComponent(out StealthSetting _))
                        {
                            a.gameObject.AddComponent<StealthSetting>().father = this;
                            if (a.GetComponent<TryConnect>().rb_2 != null)
                            {
                                a.GetComponent<TryConnect>().rb_2.gameObject.AddComponent<StealthSetting>().father = this;
                            }
                        }
                    }
                    else
                    {
                        if (a.TryGetComponent(out StealthSetting ss))
                        {
                            ss.Back();
                            Destroy(ss);
                        }
                    }
                }
            }
            else
            {
                AllBlock.RemoveAt(i);
                i--;
            }
        }
    }
}
