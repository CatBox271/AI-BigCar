using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CodeManager : MonoBehaviour
{
    public List<SwarmCode> SpecialGet;
    public List<SwarmCode> CodeList = new();
    public List<List<SwarmCode>> UpList = new();
    public int width = 10;
    bool UpFind(List<SwarmCode> s, SwarmCode sc,out int y)
    {
        y = s.FindIndex(ss => ss == sc);
        return y != -1;
    }
    public void SetUp(int towards, SwarmCode sc)
    {
        int y = -1;
        int x = UpList.FindIndex(s => UpFind(s, sc, out y));
        if (x != -1) UpList[x].RemoveAt(y);//移除原先位置

        UpList[towards].Add(sc);
    }
    public bool SetCode(int towards, SwarmCode sc)
    {
        int from = CodeList.FindIndex(s => s == sc);
        bool Veritical = false;//垂直false
        if (CodeList[towards] == null)
        {
            if (from != -1) CodeList[from] = null;
            CodeList[towards] = sc;
            return true;
        }
        else
        {
            int block = -2;
            if (from != -1)
            {
                int d = towards - from;
                if (Mathf.Abs(d) == 1 && !OnOtherSide(towards, from))
                {
                    if (d > 0)//Right
                    {
                        block = TryFindBlock(towards, 1);
                    }
                    else//Left
                    {
                        block = TryFindBlock(towards, -1);
                    }
                }
                if (Mathf.Abs(d) == width && !OnOtherSide(towards, from))
                {
                    Veritical = true;
                    if (d > 0)//Down
                    {
                        block = TryFindBlock(towards, width);
                    }
                    else//UP
                    {
                        block = TryFindBlock(towards, -width);
                    }
                }
            }
            else//没有找到,直接替换
            {
                Destroy(CodeList[towards].gameObject);
                CodeList[towards] = sc;
                return true;
            }
            if (block < 0)//并非移动一行,进行替换
            {
                Destroy(CodeList[towards].gameObject);
                CodeList[towards] = CodeList[from];
                CodeList[from] = null;
            }
            else//移动
            {
                if (Veritical)//1列
                {
                    List<int> IL = new();
                    int add = (block - from) > 0 ? width : -width;
                    for (int i = from; i != block + add; i += add)
                    {
                        IL.Add(i);
                    }
                    for (int i = IL.Count - 1; i > 0; i--)
                    {
                        CodeList[IL[i]] = CodeList[IL[i - 1]];
                    }
                    CodeList[from] = null;
                    MoveBlock(IL);
                }
                else//1行
                {
                    CodeList.RemoveAt(block);
                    CodeList.Insert(from, null);

                    List<int> IL = new();
                    int add = (block - from) > 0 ? 1 : -1;
                    for (int i = from; i != block + add; i += add)
                    {
                        IL.Add(i);
                    }
                    MoveBlock(IL);
                }
            }
            return true;
        }
    }
    void SetCodeList()
    {
        CodeList.Clear();
        UpList.Clear();
        for (int i = 0; i < width * width; i++)
        {
            CodeList.Add(null);
            UpList.Add(new());
        }
    }
    public void BackCode()
    {
        if (IC == null) return;
        StringBuilder str = new();
        SwarmCode sc;
        Vector2Int v2;
        for (int i = 0; i < width * width; i++)
        {
            str.Clear();
            for (int t = 0; t < UpList[i].Count; t++)
            {
                sc = UpList[i][t];
                if (sc == null) continue;
                if (sc.kind == "{")//只保存左扩内容
                {
                    str.Append(sc.kind);
                    v2 = sc.GetComponent<DieWith>().dieWith.GetComponent<SwarmCode>().IntPos;
                    v2.x += v2.y * 10;//把x变成0-100位数
                    if (v2.x < 10) str.Append(0);
                    str.Append(v2.x);//最终保存的样式  {09
                }
            }
            if (CodeList[i] != null) str.Append(CodeList[i].kind);
            IC.OutCode[i] = str.ToString();
        }
        IC.CreatList(false);
        OwnCar.StopTouch = false;
    }
    IC_Code IC;
    public GameObject OutPut;
    public int KeepLeft = 12;
    public void GetCode(string[] codeList, List<(string, int)> block,IC_Code ic)
    {
        IC = ic;
        Transform tf = transform.parent.GetChild(0);
        SetCodeList();
        for (int i = transform.childCount - 1; i > -1; i--)//清空重置
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        for (int i = tf.childCount - 1; i > KeepLeft; i--)//要保留一部分
        {
            DestroyImmediate(tf.GetChild(i).gameObject);
        }
        for (int i = 0; i < block.Count; i++)//生成OutPut
        {
            Instantiate(OutPut, tf).GetComponent<PicGet>().SetPic(block[i]);
        }

        for (int i = 0; i < codeList.Length; i++)
        {
            SetCodeButton(i, codeList);
            SizeButton(i);
        }
    }
    public void SetCodeButton(int i, string[] codeList, string str = null)
    {
        if (str == null) str = codeList[i];
        if (string.IsNullOrEmpty(str)) return;
        if (TryGetButton(str, out SwarmCode swarmCode))
        {
            SetCode(i, swarmCode.Copy(i));
        }
        else
        {
            if (str.StartsWith("{"))
            {
                int to = (int)float.Parse(str.Substring(1, 2));
                SwarmCode sc = SpecialGet[0].Copy(i);
                SetCode(i, sc);
                var get = sc.GetComponent<SwarmBracket>().Swarm(to);
                SetUp(i, get[0]);
                SetUp(to, get[1]);

                if (str.Length > 3)//继续看括弧里的内容
                {
                    SetCodeButton(i, codeList, str[3..]);
                }
                return;
            }
            if (str.StartsWith("Input:Num:"))
            {
                SetCode(i, SpecialGet[1].Copy(i, str.Replace("Input:Num:", "")));
                return;
            }
            if (str.StartsWith("Input:Bool:"))
            {
                SetCode(i, SpecialGet[2].Copy(i, str.Replace("Input:Bool:", "")));
                return;
            }
            if (str.StartsWith("Num:"))
            {
                SetCode(i, SpecialGet[3].Copy(i, str.Replace("Num:", "")));
                return;
            }
            if (str.StartsWith("Bool:"))
            {
                SetCode(i, SpecialGet[4].Copy(i, str.Replace("Bool:", "")));
                return;
            }
            if (str.StartsWith("Value:"))
            {
                SetCode(i, SpecialGet[5].Copy(i, str.Replace("Value:", "")));
                return;
            }
        }
    }

    bool TryGetButton(string _kind ,out SwarmCode swarmCode)
    {
        swarmCode = null;
        for (int n = 0; n < 3; n += 2)
        {
            Transform tf = transform.parent.GetChild(n);
            for (int i = 0; i < tf.childCount; i++)
            {
                SwarmCode sc = tf.GetChild(i).GetComponent<SwarmCode>();
                if (sc.kind == _kind)
                {
                    swarmCode = sc;
                    return true;
                }
            }
        }
        return false;
    }
    void MoveBlock(List<int> IL)
    {
        foreach (int s in IL)
        {
            if (CodeList[s] != null) CodeList[s].MoveTo(s);
        }
    }
    int TryFindBlock(int i, int forward)
    {
        bool jugde;
        int block = -1;
        do
        {
            jugde = false;
            if (!OnOtherSide(i, i + forward))
            {
                if (CodeList[i + forward] != null)
                {
                    jugde = true;
                }
                else
                {
                    block = i + forward;
                }
            }
            i+= forward;
        } while (jugde);
        return block;
    }
    public void SizeButton(int where)
    {
        try
        {
            UpList[where].ForEach(s => { try { s?.SizeUp(); } catch { } });
        }
        catch { };
        try
        {
            CodeList[where]?.SizeUp(); 
        } catch { };
    }
    private bool OnOtherSide(int origin, int towards)
    {
        Vector2 size = new(width, width);
        if (towards < 0 || towards > size.x * size.y - 1)
        {
            return true;
        }
        Vector2 PosA = new((int)(origin / size.x), origin % size.x);
        Vector2 PosB = new((int)(towards / size.x), towards % size.x);
        if (PosA.x == PosB.x || PosA.y == PosB.y)
        {
            return false;
        }
        return true;
    }

    public void 保存芯片预设()
    { 
    
    }
}
