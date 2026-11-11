using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockInform : MonoBehaviour
{
    public Text text;
    public string BlockName;

    public void SetName(string Name)
    {
        BlockName = Name;
        if (BlockName.Length == 0)
        {
            State = false;
        }
        else
        {
            // 名字统一走 GameManager.BlockLibrary：SwarmBlock 传进来的是预制体根物体名，
            // 也就是 BlockInfo.code_name
            int i = GameManager.GetBlockIndexByCode(BlockName);
            if (i != -1) text.text = GameManager.BlockLibrary[i].describe;
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
}
