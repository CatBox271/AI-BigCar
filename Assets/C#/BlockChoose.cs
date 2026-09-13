using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockChoose : MonoBehaviour
{
    //第一项为必定刷新，后几项随机选两项
    public List<string> Kind = new();
    public List<int> Num = new();

    public List<int> BeChoosed = new() { 0 };
    private void Awake()
    {
        if (Num.Count == 0)
        {
            Destroy(gameObject);
        }
        Fresh();
    }
    public void Fresh()
    {
        if (BeChoosed.Count == 3)
        {
            BeChoosed = new();
        }
        if (Num.Count >= 3)
        {
            for (int i = 0; i < 2; i++)
            {
                bool bb;
                int r;
                do
                {
                    r = Random.Range(1, Num.Count);
                    bb = false;
                    if (!BeChoosed.Contains(r))
                    {
                        BeChoosed.Add(r);
                        bb = true;
                    }
                } while (!bb);
            }
            for (int i = 0; i < 3; i++)
            {
                Image im = transform.GetChild(i + 1).GetChild(0).GetComponent<Image>();
                if (Kind.Count > BeChoosed[i])
                {
                    im.sprite = Resources.Load("Sprite/" + Kind[BeChoosed[i]]) as Sprite;
                    im.SetNativeSize();
                }
            }
        }

    }

    public void Down(string get_name)
    {
        int num = BeChoosed[(int)float.Parse(get_name) - 1];
        GameObject go = Resources.Load("GameObject/" + Kind[num]) as GameObject;
        transform.parent.GetComponent<BlockManager>().GetBlock(go,Num[num]);
        Destroy(gameObject);
    }
}
