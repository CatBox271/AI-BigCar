using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MV_Manager : MonoBehaviour
{
   public bool StateLoad;
    public CorrectUI cu;
    public Animator anim;
    GameManager.GameState last = GameManager.GameState.Start;
    private void Update()
    {
        Record();
        if (last == GameManager.gameState) return;
        last = GameManager.gameState;
        if (GameManager.gameState != GameManager.GameState.Ready)
        {
            cu.RelativePosition.y = 200;
            cu.RefreshPos();
        }
        else
        {
            cu.RelativePosition.y = -25;
            cu.RefreshPos();
        }
    }
    public BlockManager bm;
    private void Awake()
    {
        bm = transform.parent.GetComponent<BlockManager>();
        RecordTimes = 0;
        ownTimes = 0;

        GameManager.layIn = 0;
        GameManager.DeleteMode = false;
    }
    List<List<string>> UndoRecord = new();
    List<List<string>> RedoRecord = new();

    public static void GetRecord()
    {
        RecordTimes++;
    }

    public static int RecordTimes = 0;

    public int ownTimes = 0;
    public void LayInChange()
    {
        GameManager.layIn = GameManager.layIn switch
        {
            0 => 1,
            _ => 0,
        };
        transform.GetChild(9).GetComponent<Image>().color = GameManager.layIn == 1 ? Color.yellow : Color.green;
        transform.GetChild(9).GetChild(0).GetComponent<Text>().text = GameManager.layIn == 1 ? "放置于上层": "放置于下层";
    }
    public void DeleteMode()
    {
        GameManager.DeleteMode = !GameManager.DeleteMode;
        transform.GetChild(8).GetComponent<Image>().color = GameManager.DeleteMode ? Color.red : Color.green;
        transform.GetChild(8).GetChild(0).GetComponent<Text>().text = GameManager.DeleteMode ? "关闭删除模式" : "开启删除模式";
    }

    public void SaveDesign()
    {
        List<string> get = bm.KeepLevel();
        int carIndex = 0;
        List<OwnCar> oc = bm.FindCar();

        List<List<string>> allcar = new();
        List<List<string>> RealDesign = new();
        for (int i = 0; i < get.Count; i++)
        {
            if (allcar.Count <= carIndex)
            {
                allcar.Add(new List<string>());
            }
            if (get[i] == "114514")
            {
                carIndex++;
            }
            else
            {
                allcar[carIndex].Add(get[i]);
            }
        }

        for (int i = 0; i < oc.Count; i++)
        {
            int width = oc[i].ColliderArea[0];
            int height = oc[i].ColliderArea[1];
            int a = width -1; int b = 0; int c = height -1; int d = 0;
            int BlockIndex = 0;
            for (int ii = 0; ii < allcar[i].Count; ii++)
            {
                if (allcar[i][ii] == "StartCode")
                {
                    ii += 100;
                    continue;
                }
                if (float.Parse( allcar[i][ii] )>= 0 && float.Parse(allcar[i][ii]) < 1000)
                {
                    if (float.Parse(allcar[i][ii]) != 0)
                    {
                        int x = BlockIndex % width;
                        int y = BlockIndex / width;
                        if (a > x) a = x;
                        if (b < x) b = x;
                        if (c > y) c = y;
                        if (d < y) d = y;
                    }
                    BlockIndex++;
                }
            }
            BlockIndex = -1;
            int t = 0;
            for (int ii = a + c * width; ii <= b + d * width; ii++)
            {
                //换为精简保存方式

                if (RealDesign.Count <= i)
                {
                    RealDesign.Add(new List<string>() {a.ToString(),b.ToString(),c.ToString(),d.ToString()});//前一项是长
                }

                do
                {
                    if (float.Parse(allcar[i][t]) >= 0 && float.Parse(allcar[i][t]) < 1000)
                    {
                        BlockIndex++;
                    }
                    if (BlockIndex <= ii)
                    {
                        if (BlockIndex == ii)
                        {
                            RealDesign[i].Add(allcar[i][t]);
                            if (Mathf.Abs(float.Parse(allcar[i][t])) == 46 && allcar[i][t + 1] == "StartCode")
                            {
                                for (int p = 1; p < 102; p++)
                                {
                                    RealDesign[i].Add(allcar[i][t + p]);
                                }
                                t += 101;
                            }
                        }
                        t++;
                    }
                } while (BlockIndex <= ii && t < allcar[i].Count);

                if (BlockIndex > ii)
                {
                    BlockIndex--;
                }

                if (ii % width == b)
                {
                    ii += width - (b - a) - 1;
                }
            }
        }//所有的载具已经简化存于RealDesign中
        RDR .Clear();
        RDR.AddRange(RealDesign);
        //显示动画
        anim.Play("SaveDesign");
    }
    public List<List<string>> RDR = new(); //RealDesign 的 记录




    public void Record()
    {
        if (ownTimes < RecordTimes)
        {
            ownTimes++;
            UndoRecord.Add(new List<string>());
            List<string> get = bm.KeepLevel();
            for (int i = 0; i < get.Count; i++)
            {
                UndoRecord[^1].Add(get[i]);
            }

        }
        transform.GetChild(1).GetComponent<Button>().interactable = UndoRecord.Count >= 2;
        transform.GetChild(2).GetComponent<Button>().interactable = RedoRecord.Count >= 1;
    }
    public void Enpty()
    {
        List<OwnCar> oc = bm.FindCar();
        for (int o = 0; o < oc.Count; o++)
        {
            List<List<GameObject>> gll = new();
            for (int i = 0; i < oc[o].AllObject.Count; i++)
            {
                gll.Add(oc[o].AllObject[i]);
            }
            for (int i = 0; i < gll.Count; i++)
            {
                for (int ii = 0; ii < gll[i].Count; ii++)
                {
                    if (gll[i][ii] != null)
                    {
                        string n = gll[i][ii].name;
                        GameObject kind = Resources.Load("GameObject/" + n) as GameObject;
                        bm.GetBlock(kind, 1);
                    }
                }
            }
            oc[o].ClearCar();
        }
        bm.KeepLevel();
        bm.MV_Special();

        GetRecord();
    }
    public void DoTo(List<string> ChangeTo)
    {


        GetRecord();
        Record();

        CompareChange(bm.save, ChangeTo);

        bm.FindCar().ForEach(s => s.ClearCar());
        bm.save.Clear();
        for (int i = 0; i < ChangeTo.Count; i++)
        {
            bm.save.Add(ChangeTo[i]);
        }

        bm.MV_Special();
        GetRecord();
    }
    public void DoTo(int add)
    {
        bm.FindCar().ForEach(s => s.ClearCar());
        if (add == -1)
        {

            CompareChange(bm.save, UndoRecord[^2]);

            bm.save .Clear();
            for (int i = 0; i < UndoRecord[^2].Count; i++)
            {
                bm.save.Add(UndoRecord[^2][i]);
            }




            RedoRecord.Add(UndoRecord[^1]);
            UndoRecord.RemoveAt(UndoRecord.Count - 1);
            UndoRecord.RemoveAt(UndoRecord.Count - 1);

            bm.MV_Special();
            GetRecord();

        }
        else
        {
            CompareChange(bm.save, RedoRecord[^1]);

            bm.save .Clear();
            for (int i = 0; i < RedoRecord[^1].Count; i++)
            {
                bm.save.Add(RedoRecord[^1][i]);
            }

            UndoRecord.Add(RedoRecord[^1]);
            RedoRecord.RemoveAt(RedoRecord.Count - 1);

            bm.MV_Special();
        }
    }

    void CompareChange(List<string>now ,List<string> towards)
    {
        List<string> nowItem = new();
        List<string> toItem = new();
        for (int a = 0; a < now.Count; a++)
        {
            if (Mathf.Abs(float.Parse( now[a])) < 100f)
            {
                float add = Mathf.Abs(float.Parse(now[a]));
                if (add != 0)
                {
                    nowItem.Add(GameManager.BlockIndex[(int)add]);
                }
                if (add == 46) a += 101;
            }
        }
        for (int a = 0; a < towards.Count; a++)
        {
            if (Mathf.Abs(float.Parse(towards[a]) )< 100f)
            {
                float add = Mathf.Abs(float.Parse(towards[a]));
                if (add != 0)
                {
                    toItem.Add(GameManager.BlockIndex[(int)add]);
                }
                if (add == 46) a += 101;
            }
        }
        for (int p = nowItem.Count - 1;p>-1 ; p--)
        {
            if (toItem.Remove(nowItem[p]))
            {
                nowItem.RemoveAt(p);
            }
        }
        for (int i = 0; i < nowItem.Count; i++)
        {
            GameObject kind = Resources.Load("GameObject/" + nowItem[i]) as GameObject;
            bm.GetBlock(kind, 1);
        }
        for (int i = 0; i < toItem.Count; i++)
        {
            GameObject kind = Resources.Load("GameObject/" + toItem[i]) as GameObject;
            bm.GetBlock(kind, -1);
        }
    }

    public void MoveVehicle(int towards)//0U 1D 2L 3R
    {
        List<OwnCar> oc = bm.FindCar();
        bool pass = false;
        for (int o = 0; o < oc.Count; o++)
        {
            List<List<GameObject>> gll = new();
            for (int i = 0; i < oc[o].AllObject.Count; i++)
            {
                gll.Add(oc[o].AllObject[i]);
            }
            int width = oc[o].ColliderArea[0];
            int Height = oc[o].ColliderArea[1];
            //通过修改all_cars的顺序，再保存重新加载
            List<List<GameObject>> NeedCheck = new();
            switch (towards)
            {
                case 0:
                    for (int i = 0; i < width; i++)
                    {
                        NeedCheck.Add(gll[gll.Count - width + i]);
                    }
                    break;
                case 1:
                    for (int i = 0; i < width; i++)
                    {
                        NeedCheck.Add(gll[i]);
                    }
                    break;
                case 2:
                    for (int i = 0; i < Height; i++)
                    {
                        NeedCheck.Add(gll[i * width]);
                    }
                    break;
                case 3:
                    for (int i = 0; i < Height; i++)
                    {
                        NeedCheck.Add(gll[(i + 1) * width - 1]);
                    }
                    break;
            }
            bool ok = true;
            for (int i = 0; i < NeedCheck.Count; i++)
            {
                if (NeedCheck[i].Count != 0)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                switch (towards)
                {
                    case 0:
                        for (int i = 0; i < width; i++)
                        {
                            gll.RemoveAt(gll.Count - 1);
                        }
                        gll.InsertRange(0, NeedCheck);
                        break;
                    case 1:
                        for (int i = 0; i < width; i++)
                        {
                            gll.RemoveAt(0);
                        }
                        gll.AddRange(NeedCheck);
                        break;
                    case 2:
                        for (int i = 0; i < Height; i++)
                        {
                            gll.RemoveAt(i * width);
                            gll.Insert((i + 1) * width - 1, NeedCheck[i]);
                        }
                        break;
                    case 3:
                        for (int i = 0; i < Height; i++)
                        {
                            gll.RemoveAt((i + 1) * width - 1);
                            gll.Insert(i * width, NeedCheck[i]);
                        }
                        break;
                }
                oc[o].AllObject.Clear();
                for (int i = 0; i < gll.Count; i++)
                {
                    oc[o].AllObject.Add(gll[i]);
                }
                pass = true;
                oc[o].MoveCar(towards switch
                {
                    0 => new(0, 1),
                    1 => new(0, -1),
                    2 => new(-1, 0),
                    3 => new(1, 0),
                });
            }
        }
        if (pass)
        {
            bm.KeepLevel();
            GetRecord();
        }
    }

}
