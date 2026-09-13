using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompressionTank : MonoBehaviour
{
    public float Contain = 120;
    public float value = 0;

    public float speed = 10;
    public List<Balloon> AllBlock = new();

    public GameObject Pointer;
    public int State;
    public void Search()
    {
        AllBlock = new();
        GetComponent<CheckAlive>().Search(new List<string>() { "Balloon","-Block" }, true, new()).ForEach(s => AllBlock.Add(s.GetComponent<Balloon>()));
    }

    int first = 4;

    private void Awake()
    {
        first += Random.Range(0, 3);
    }
    private void FixedUpdate()
    {
        Pointer.transform.localEulerAngles = new Vector3(0, 0, value/Contain  * -360);
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            first--;
            if (first == 0)
            {
                Search();
            }
            if (AllBlock.Count == 0) return;
            if (State != 0)
            {
                if (State == 1)
                {
                    float Speed = Time.fixedDeltaTime * speed / AllBlock.Count;
                    if ((Contain - value) >= Speed * AllBlock.Count)
                    {
                        for (int i = AllBlock.Count - 1; i > -1; i--)
                        {
                            if (AllBlock[i] != null)
                            {
                                if (AllBlock[i].value >= Speed)
                                {
                                    AllBlock[i].value -= Speed;
                                    value += Speed;
                                }
                                else
                                {
                                    value += AllBlock[i].value;
                                    AllBlock[i].value = 0;
                                }
                            }
                            else
                            {
                                AllBlock.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        float every = (Contain - value) / AllBlock.Count;
                        for (int i = AllBlock.Count - 1; i > -1; i--)
                        {
                            if (AllBlock[i] != null)
                            {
                                if (AllBlock[i].value >= every)
                                {
                                    AllBlock[i].value -= every;
                                    value += every;
                                }
                                else
                                {
                                    value += AllBlock[i].value;
                                    AllBlock[i].value = 0;
                                }
                            }
                            else
                            {
                                AllBlock.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    float Speed = Time.fixedDeltaTime * speed /AllBlock.Count;
                    if (value >= Speed * AllBlock.Count)
                    {
                        for (int i = AllBlock.Count - 1; i > -1; i--)
                        {
                            if (AllBlock[i] != null)
                            {
                                if (AllBlock[i].max_value - AllBlock[i].value >= Speed)
                                {
                                    AllBlock[i].value += Speed;
                                    value -= Speed;
                                }
                                else
                                {
                                    value -= AllBlock[i].max_value - AllBlock[i].value;
                                    AllBlock[i].value = AllBlock[i].max_value;
                                }
                            }
                            else
                            {
                                AllBlock.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        float every = value / AllBlock.Count;
                        for (int i = AllBlock.Count - 1; i > -1; i--)
                        {
                            if (AllBlock[i] != null)
                            {
                                if (AllBlock[i].max_value - AllBlock[i].value >= every)
                                {
                                    AllBlock[i].value += every;
                                    value -= every;
                                }
                                else
                                {
                                    value -= AllBlock[i].max_value - AllBlock[i].value;
                                    AllBlock[i].value = AllBlock[i].max_value;
                                }
                            }
                            else
                            {
                                AllBlock.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
    }
}
