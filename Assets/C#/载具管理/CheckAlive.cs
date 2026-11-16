using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CheckAlive : MonoBehaviour
{
    public List<FixedJoint2D> allConnect = new();
    public List<CheckAlive> CAList = new();
    public List<int> BlockOffset = new();//0上1下2左3右4中
    public List<Vector2Int> FacePair = new();//与 allConnect 一一对应：这条连接两侧的对偶面（DualCut.NoFace = 不分割平面）

    public struct CutEdge : DualCut.ICutEdge
    {
        public int A, B;
        public CheckAlive peer;
        public readonly int FaceA => A;
        public readonly int FaceB => B;
    }
    public List<CutEdge> cuts = new();//本块身上已经断掉的连接，判环只用这几条
    public int gridW, gridH;
    public static bool DualCutOn = true;//对偶判据开关（关掉就走原来的全量 Search）

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
            if (allConnect[i] == null)
            {
                RecordCut(i);
                RemoveListAt(i);
            }
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
        if (DualCutOn && DualTrySplit(out int splitVid))
        {
            ReconnectCA(VID);
            if (splitVid != VID) ReconnectCA(splitVid);
            GameManager.Check();
            ReconnectStart(VID);
            if (splitVid != VID) ReconnectStart(splitVid);
            return;
        }
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
        //这一轮已经按真实连接重算过，跨车的断边就没用了，留着会让下一轮重复切
        for (int i = 0; i < GameManager.allvehicle[LID].Count; i++)
        {
            CheckAlive ca = GameManager.allvehicle[LID][i];
            if (ca == null) continue;
            ca.cuts.RemoveAll(c => c.peer == null || c.peer.VID != ca.VID);
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
        if (i < FacePair.Count) FacePair.RemoveAt(i);

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

    //记下一条断开的连接（对偶判据只看这几条，跟载具多大无关）
    void RecordCut(int i)
    {
        if (i >= FacePair.Count) return;
        Vector2Int fp = FacePair[i];
        if (fp.x == DualCut.NoFace) return;
        CheckAlive peer = i < CAList.Count ? CAList[i] : null;
        //同一条连接会被两头各扫到一次，只让 InstanceID 小的那侧记，否则等于记了两条重边、判环会假成环
        if (peer != null && GetInstanceID() < peer.GetInstanceID()) return;
        cuts.Add(new CutEdge { A = fp.x, B = fp.y, peer = peer });
    }

    static readonly HashSet<CheckAlive> sideVisited = new();
    static readonly Queue<CheckAlive> sideQueue = new();
    static readonly List<CheckAlive> sideBuffer = new();
    static readonly HashSet<CheckAlive> sideSet = new();
    static readonly List<CutEdge> carCuts = new();

    //对偶切分后的收尾：先 CA 自己，再其它 IStart（顺序与 BrokenCheck 原流程一致）
    static void ReconnectCA(int vid)
    {
        if (vid < 0 || vid >= GameManager.allvehicle.Count) return;
        List<CheckAlive> L = GameManager.allvehicle[vid];
        for (int i = 0; i < L.Count; i++)
        {
            CheckAlive ca = L[i];
            if (ca != null) ca.SetStart(Time.time);
        }
    }
    static void ReconnectStart(int vid)
    {
        if (vid < 0 || vid >= GameManager.allvehicle.Count) return;
        List<CheckAlive> L = GameManager.allvehicle[vid];
        for (int i = 0; i < L.Count; i++)
        {
            CheckAlive ca = L[i];
            if (ca == null) continue;
            if (ca.TryGetComponent(out IStart reconnect)) reconnect.SetStart(Time.time);
        }
    }

    //断边只记在“先扫到的那一侧”身上，所以两头都要看
    bool IsCutBetween(CheckAlive a, CheckAlive b)
    {
        for (int i = 0; i < a.cuts.Count; i++) if (a.cuts[i].peer == b) return true;
        for (int i = 0; i < b.cuts.Count; i++) if (b.cuts[i].peer == a) return true;
        return false;
    }

    /// <summary>
    /// 对偶判据判一次：断边成环才把小侧切出去，返回 true；不成环返回 false，什么都没动。
    /// 要让它自动生效，在 BrokenCheck 开头加一句 if (DualCutOn && DualTrySplit()) return;（收尾的 GameManager.Check 与 IStart.SetStart 仍需自己跑）。
    /// </summary>
    //Editor 里选中任意方块，右键组件跑一下：看当前车的断边能不能成环
    [ContextMenu("对偶判据自检")]
    void DualSelfTest()
    {
        int vid = VID;
        if (vid < 0 || vid >= GameManager.allvehicle.Count)
        {
            Debug.Log($"[DualCut] VID={vid} 无效", this);
            return;
        }
        List<CheckAlive> car = GameManager.allvehicle[vid];
        carCuts.Clear();
        int noFace = 0;
        for (int i = 0; i < car.Count; i++)
        {
            CheckAlive ca = car[i];
            if (ca == null) continue;
            for (int k = 0; k < ca.cuts.Count; k++)
            {
                CutEdge ce = ca.cuts[k];
                if (ce.peer == null) continue;
                carCuts.Add(ce);
            }
        }
        for (int i = 0; i < car.Count; i++)
        {
            if (car[i] != null && (car[i].gridW < 2 || car[i].gridH < 2)) noFace++;
        }
        bool cyc = DualCut.HasCycle(carCuts);
        Debug.Log($"[DualCut] 车 {vid}：{car.Count} 块，断边 {carCuts.Count} 条，面 id 缺失 {noFace} 块，成环 = {cyc}", this);
        for (int i = 0; i < carCuts.Count; i++)
        {
            Debug.Log($"  断边 {i}：面 {carCuts[i].FaceA} — {carCuts[i].FaceB}（对端 {carCuts[i].peer.name}）", this);
        }
    }

    public bool DualTrySplit(out int smallVid)
    {
        smallVid = -1;
        if (VID < 0 || VID >= GameManager.allvehicle.Count) return false;
        List<CheckAlive> car = GameManager.allvehicle[VID];
        if (car == null || car.Count < 2) return false;

        //断边散在各块身上，先收全车的
        carCuts.Clear();
        for (int i = 0; i < car.Count; i++)
        {
            CheckAlive ca = car[i];
            if (ca == null || ca.VID != VID) continue;
            for (int k = 0; k < ca.cuts.Count; k++)
            {
                CutEdge ce = ca.cuts[k];
                if (ce.peer == null || ce.peer.VID != VID) continue;//已经分出去的那一半不算
                carCuts.Add(ce);
            }
        }
        if (carCuts.Count == 0) return false;
        if (!DualCut.HasCycle(carCuts)) return false;

        CheckAlive seed = null;
        for (int i = 0; i < carCuts.Count; i++)
        {
            if (carCuts[i].peer != null) { seed = carCuts[i].peer; break; }
        }
        if (seed == null) return false;

        DualCut.CollectSide(seed, n => n.CAList, IsCutBetween, sideVisited, sideQueue, sideBuffer);
        if (sideBuffer.Count == 0 || sideBuffer.Count >= car.Count) return false;

        List<CheckAlive> small = sideBuffer;
        if (small.Count * 2 > car.Count)
        {
            sideSet.Clear();
            for (int i = 0; i < small.Count; i++) sideSet.Add(small[i]);
            small = new List<CheckAlive>();
            for (int i = 0; i < car.Count; i++)
            {
                if (car[i] != null && !sideSet.Contains(car[i])) small.Add(car[i]);
            }
            if (small.Count == 0) return false;
        }

        //绕过 VehicleSet 的“同一帧只算一次”
        for (int i = 0; i < small.Count; i++) if (small[i] != null) small[i].T = -1f;
        small[0].VehicleSet(Time.time);

        //大侧成员表去掉已经分出去的那些，并把跨车的断边清掉（留在同侧的继续参与下次判环）
        int bigVid = VID, newVid = small[0].VID;
        smallVid = newVid;
        car.RemoveAll(c => c == null || c.VID != bigVid);
        for (int i = 0; i < car.Count; i++)
        {
            CheckAlive ca = car[i];
            ca.cuts.RemoveAll(c => c.peer == null || c.peer.VID != bigVid);
        }
        for (int i = 0; i < small.Count; i++)
        {
            CheckAlive ca = small[i];
            if (ca == null) continue;
            ca.cuts.RemoveAll(c => c.peer == null || c.peer.VID != newVid);
        }
        return true;
    }
}
