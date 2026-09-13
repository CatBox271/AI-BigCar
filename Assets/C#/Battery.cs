using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    public float value;
    public float MaxValue;
    public float Producing;

    public float SpriteHeight;
    public SpriteRenderer ShowLeft;


    private void OnValidate()
    {
        if (ShowLeft != null)
        {
            ShowLeft.size = new(ShowLeft.size.x, value / MaxValue* SpriteHeight);
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
            StartCoroutine(nameof (Late));

        }
    }

    IEnumerator Late()
    {
        yield return new WaitForFixedUpdate();
        Add();
    }
    public void Add()
    {
        float t = Time.fixedDeltaTime;
        value += Producing * t;

        if (MaxValue < value)
        {
            float more = value - MaxValue;
            foreach (Battery ba in AllBlock)
            {
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
            value = MaxValue;
        }
        if (ShowLeft != null)
        {
            ShowLeft.size = new(ShowLeft.size.x, value / MaxValue * SpriteHeight);
        }
    }

    public List<Battery> AllBlock = new();
    public void Search()
    {
        if (AllBlock.Count == 0)
        {
            GetComponent<CheckAlive>().Search(new List<string>() { "Battery" }, true, new()).ForEach(s => AllBlock.Add(s.GetComponent<Battery>()));
        }
        SetValue();
    }

    private void SetValue()
    {
        List<Battery> SameEngine = new();

        for (int i = 0; i < AllBlock.Count; i++)
        {
            Battery a = AllBlock[i];
            if (a != null)
            {
                if (a.name.Contains("Engine"))
                {
                    SameEngine.Add(a.GetComponent<Battery>());
                    AllBlock.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        for (int i = 0; i < SameEngine.Count; i++)
        {
            if (SameEngine[i].AllBlock.Count == 0)
            {
                SameEngine[i].AllBlock.AddRange(AllBlock);
            }
        }
    }
}
