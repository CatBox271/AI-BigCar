using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CheckAlive : MonoBehaviour
{
    public List<FixedJoint2D> allConnect = new();
    public List<CheckAlive> CAList = new();
    public List<int> BlockOffset = new();//0上1下2左3右4中

    public int VID = -1;

    bool brokenDirty;//有连接失效，等本帧统一重算一次
    int visitMark;
    int filterMark;
    static int searchId;
    static int filterId;
    int tick;

    private void FixedUpdate()
    {
        tick++;
        if (tick < 8 && !brokenDirty) return;
        tick = 0;
        for (int i = allConnect.Count - 1; i > -1; i--)
        {
            if (allConnect[i] == null) RemoveListAt(i);
        }
        for (int i = CAList.Count - 1; i > -1; i--)
        {
            if (CAList[i] == null) RemoveListAt(i);
        }
        if (!brokenDirty) return;
        brokenDirty = false;
        BrokenCheck();
    }
    public void ReadyBrokenCheck()
    {
        brokenDirty = true;
    }

    void BrokenCheck()
    {
        int LID = VID;
        List<CheckAlive> keep = new();
        keep.AddRange(GameManager.allvehicle[LID]);
        GameManager.allvehicle[LID].Clear();
        GameManager.allvehicle[LID].AddRange(Search());

        for (int i = keep.Count - 1; i > -1; i--)
        {
            if (keep[i] == null)
            {
                keep.RemoveAt(i);
                continue;
            }
            if (keep[i].visitMark == searchId) continue;//还在本帧的连接图里
            //把断开连接的都分离了
            keep[i].VehicleSet(Time.time);
        }
        for (int i = 0; i < keep.Count; i++)
        {
            if (keep[i].TryGetComponent(out CheckAlive reconnect)) reconnect.SetStart(Time.time);
        }
        //提醒GameManager对信号系统更新
        GameManager.Check();
        for (int i = 0; i < keep.Count; i++)
        {
            if (keep[i].TryGetComponent(out IStart reconnect)) reconnect.SetStart(Time.time);
        }
    }
    float T;
    public void VehicleSet(float t)
    {
        if (t == T) return;
        T = t;
        List<CheckAlive> L = new();
        L.AddRange(Search());//Search 返回的是静态共享列表，这里必须复制，否则 allvehicle 会存到共享引用

        bool hasCenter = false;
        for (int i = 0; i < L.Count; i++)
        {
            if (!L[i].CompareTag("Center")) continue;
            hasCenter = true;
            break;
        }
        if (!hasCenter)
        {
            //给所有不存在的缓死
            for (int i = 0; i < L.Count; i++) Destroy(L[i].gameObject, 0.5f);
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
        for (int i = 0; i < L.Count; i++)//跳过连接的
        {
            L[i].T = T;
            L[i].VID = VID;
        }
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

        CheckAlive target = GameManager.allvehicle[VID].Find(s => s != null);
        if (target != null) target.brokenDirty = true;
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
        if (start)
        {
            SP.Clear();
            searchId++;
        }
        if (visitMark == searchId) return SP;
        visitMark = searchId;
        SP.Add(this);
        for (int i = 0; i < CAList.Count; i++)
        {
            if (CAList[i] == null) continue;
            if (allConnect[i] == null) continue;
            if (allConnect[i].connectedBody == null) continue;
            CAList[i].Search(false);
        }
        return SP;
    }
    static List<CheckAlive> SF = new();
    public List<CheckAlive> Search(List<string>nameof,bool PartOfName,  List<string> exclude, bool start = true)
    {
        if (start)
        {
            SF.Clear();
            filterId++;
        }
        if (filterMark == filterId) return SF;
        filterMark = filterId;
        for (int i = 0; i < CAList.Count; i++)
        {
            if (CAList[i] == null) continue;
            if (allConnect[i] == null) continue;
            if (allConnect[i].connectedBody == null) continue;
            if (exclude.Contains(CAList[i].name)) continue;

            if (CAList[i].filterMark != filterId)
            {
                if ((!PartOfName && nameof.Contains(CAList[i].name)) || (PartOfName && nameof.Exists(s => CAList[i].name.Contains(s))))
                {
                    SF.Add(CAList[i]);
                }
            }
            CAList[i].Search(nameof, PartOfName, exclude, false);
        }
        return  SF;
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
