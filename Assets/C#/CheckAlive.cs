using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CheckAlive : MonoBehaviour
{
    public List<FixedJoint2D> allConnect = new();
    public List<CheckAlive> CAList = new();
    public List<int> BlockOffset = new();//0上1下2左3右4中

    public int VID = -1;

    private void FixedUpdate()
    {
        for (int i = allConnect.Count - 1; i > -1; i--)
        {
            if (allConnect[i] == null) RemoveListAt(i);
        }
        for (int i = CAList.Count - 1; i > -1; i--)
        {
            if (CAList[i] == null) RemoveListAt(i);
        }
    }
    public void ReadyBrokenCheck()
    {
        BrokenCheck();
    }

    void BrokenCheck()
    {
        int LID = VID;
        List<CheckAlive> keep = new();
        keep.AddRange(GameManager.allvehicle[LID]);
        List<CheckAlive> L = new();
        L.AddRange(Search());
        GameManager.allvehicle[LID].Clear();
        GameManager.allvehicle[LID].AddRange(L);

        for (int i = keep.Count - 1; i > -1; i--)
        {
            if (keep[i] == null)
            {
                keep.RemoveAt(i);
                continue;
            }
            if (L.Contains(keep[i])) continue;
            //把断开连接的都分离了
            keep[i].VehicleSet(Time.time);
        }
        keep.ForEach(s =>
        {
            if (s.TryGetComponent(out CheckAlive reconnect)) reconnect.SetStart(Time.time);
        });
        //提醒GameManager对信号系统更新
        GameManager.Check();
        keep.ForEach(s =>
        {
            if (s.TryGetComponent(out IStart reconnect)) reconnect.SetStart(Time.time);
        });
    }
    float T;
    public void VehicleSet(float t)
    {
        if (t == T) return;
        T = t;
        List<CheckAlive> L = new();
        L.AddRange(Search());

        if (!L.Exists(s => s.CompareTag("Center")))
        {
            //给所有不存在的缓死
            L.ForEach(s => Destroy(s.gameObject, 0.5f));
        }

        //将自己添加至信息
        int NID = GameManager.allvehicle.FindIndex(s => s == null || s.TrueForAll(ss => ss == null));

        if (NID != -1)//利用久列表
        {
            GameManager.allvehicle[NID].Clear();
            VID = NID;
            GameManager.allvehicle[VID].AddRange(L);
        }
        else//增加新列表
        {
            GameManager.allvehicle.Add(L);
            VID = GameManager.allvehicle.Count - 1;
        }
        L.ForEach(s =>//跳过连接的
        {
            s.T = T;
            s.VID = VID;
        });
    }
    void VIDChange(int DID,int FID)
    {
        for (int i = 0; i < GameManager.allvehicle[DID].Count; i++)
        {
            if (GameManager.allvehicle[DID][i] != null)
            {
                GameManager.allvehicle[DID][i].VID = FID;
            }
        }
    }
    void Combined(int VID, List<CheckAlive> A)
    {
        for (int i = 0; i < A.Count; i++)//增加信息,并确保没有重复
        {
            if (A[i] == null) continue;
            if (!GameManager.allvehicle[VID].Contains(A[i])) GameManager.allvehicle[VID].Add(A[i]);
        }
    }


    void RemoveListAt(int i)
    {
        allConnect.RemoveAt(i);
        CAList.RemoveAt(i);
        BlockOffset.RemoveAt(i);

        GameManager.allvehicle[VID].Find(s=>s != null).ReadyBrokenCheck();
    }
    public bool[] Check(List<CheckAlive> Pass, List<CheckAlive> aim)
    {
        Pass.Add(this);
        bool[] BL = new bool[aim.Count];
        int ct;
        for (int i = 0; i < CAList.Count; i++)
        {
            if (Pass.Contains(CAList[i])) continue;
            if (CAList[i] == null) continue;
            if (allConnect[i] == null) continue;
            if (allConnect[i].connectedBody == null) continue;
            ct = aim.IndexOf(CAList[i]);
            if (ct != -1) BL[ct] = true;

            var NBL = CAList[i].Check(Pass, aim);
            for (int p = 0; p < aim.Count; p++)
            {
                BL[p] = BL[p] || NBL[p];
            }
        }
        return BL;
    }

    static List<CheckAlive> SP = new();
    public List<CheckAlive> Search(bool start = true)//全部查找
    {
        if (start) SP.Clear();
        SP.Add(this);
        for (int i = 0; i < CAList.Count; i++)
        {
            if (SP.Contains(CAList[i])) continue;
            if (CAList[i] == null) continue;
            if (allConnect[i] == null) continue;
            if (allConnect[i].connectedBody == null) continue;
            CAList[i].Search(false);
        }
        return SP;
    }
    public List<CheckAlive> Search(List<string>nameof,bool PartOfName,  List<string> exclude, bool start = true)
    {
        if (start) SP.Clear();
        List<CheckAlive> Final = new();
        for (int i = 0; i < CAList.Count; i++)
        {
            if (SP.Contains(CAList[i])) continue;
            if (CAList[i] == null) continue;
            if (allConnect[i] == null) continue;
            if (allConnect[i].connectedBody == null) continue;
            if (exclude.Contains(CAList[i].name))
            {
                continue;
            }

            if ((!PartOfName && nameof.Contains(CAList[i].name)) || (PartOfName && nameof.Exists(s => CAList[i].name.Contains(s))))
            {
                Final.Add(CAList[i]);
            }
            Final.AddRange(CAList[i].Search(nameof, PartOfName, exclude, false));
        }
        return  Final;
    }

    public void SetStart(float t)
    {
        VehicleSet(t);
    }
    public bool TrueAndGet(int i,out CheckAlive ca)
    {
        int k = BlockOffset.IndexOf(i);
        if (k != -1)
        {
            ca = CAList[k];
        }
        else
        {
            ca = null;
        }
        return ca != null;
    }
}
