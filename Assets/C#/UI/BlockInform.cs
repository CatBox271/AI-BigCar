using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockDictionary
{
    public string[] ALL;
}
public class BlockInform : MonoBehaviour
{
    
    public Text text;
    public string BlockName;

    public List<string> NameList = new();
    public List<string> InformList = new();

    public void SetName(string Name)
    {
        BlockName = Name;
        if (BlockName.Length == 0)
        {
            State = false;
        }
        else
            {
            if (NameList.Count == 0)
            {
                Load();
               
            }
            int i = NameList.IndexOf(BlockName);
            if (i != -1)
            text.text = InformList[i];
            State = true;
        }
    }
    bool State;
    float t;
    private void Update()
    {
        if (State)
        {
            t += Time.deltaTime * 5f;
            t = Mathf.Min(t, 1);
        }
        else 
        {
            t -= Time.deltaTime * 5f;
            t = Mathf.Max(t, 0);
        }
        rt.sizeDelta = new(2250f, 200f *  Mathf.Pow(t,0.5f));
    }
    RectTransform rt;

    private void Awake()
    {
        TryGetComponent(out rt);
    }
    public void Load()
    {
        string json = Resources.Load("BlockDictionary").ToString();
        var test = JsonUtility.FromJson<BlockDictionary>(json);

        for (int i = 0; i < test.ALL.Length; i += 2)
        {
            NameList.Add(test.ALL[i]);
            InformList.Add(test.ALL[i+1]);
        }
    }
}
