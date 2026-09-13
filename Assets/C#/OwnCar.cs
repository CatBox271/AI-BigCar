using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnCar : MonoBehaviour
{
    public int Camp;
    public List<List<GameObject>> AllObject = new();
    public Vector2Int ColliderArea;
    public  LevelCamera LC;
    float lastTime;
    public int CenterCount = 0;

    public SpriteRenderer sp;
    public Transform VideoSwarmPoint;

    private void Awake()
    {
        LC = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<LevelCamera>();
        OwnCar.StopTouch = false;
    }

    public void Swarm()//按钮按下时的接口
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).childCount >= 2)
            {
                Transform center = transform.GetChild(i).GetChild(1);
                Transform FC = transform.GetChild(i).GetChild(0);//Father of Center

                FC.tag = "Fixed";
                if (center.CompareTag("Center"))
                {
                    LC.all_cars.Add(FC);
                    center.parent = FC;
                }
            }
        }
        foreach (TryConnect rb in FindTryConnect(transform.parent))
        {
            rb.SetStart(Camp);
        }
    }
    public bool Sending;

    IEnumerator SendCarNum(int Times)
    {
        if (Camp == 2)
        {
            SelectBall.RedSending = true;
        }
        else
        {
            SelectBall.BlueSending = true;
        }
        var carlist = BlockManager.bm.FindCar();
        load_save = new();
        load_save.AddRange(BlockManager.bm.KeepLevel(carlist.IndexOf(this)));
        Sending = true;
        ClearFloatBlock();

        //循环
        for (int t = 0; t < Times; t++)
        {
            //车辆生成
            CarStart();

            yield return null;
            yield return null;
            yield return null;

            if (t + 1 != Times)
            {
                ReloadCar();
                yield return new WaitForSeconds(0.5f);
            }
        }
        Sending = false;
        CarNum = 1;
        if (Camp == 2)
        {
            SelectBall.RedSending = false;
            GetCarDesign.red_choosed = false;
        }
        else
        {
            SelectBall.BlueSending = false;
            GetCarDesign.blue_choosed = false;
        }
    }
    public float SpeedUP = 1f;
    public float HeathUP = 0f;
    public float HitUP = 0f;
    public float RangeUP = 0f;
    public int CarNum = 0;
    void EnablePower(Transform tf)
    {
        if (!GetT(tf.gameObject, out TryConnect tc)) return;
        if (tc.TryGetComponent(out Propellers p))
        {
            p.OnSet = true;
            if (p.Force > 0)
            {
                p.Force += SpeedUP * 0.75f;
            }
            else
            {
                p.Force -= SpeedUP * 0.75f;
            }
        }
        if (tc.TryGetComponent(out TNT tnt))
        {
            tnt.Hit += HitUP * 2f;
            tnt.Radius += RangeUP *0.5f;
        }
        if (tc.TryGetComponent(out Sail sail))
        {
            sail.Set(Camp == 1, 1);
            sail.value += SpeedUP * 0.5f;
        }
        if (tc.TryGetComponent(out Stealth st))
        {
            st.first = 3;
        }
        if (tc.TryGetComponent(out PowerWheel wheel))
        {
            wheel.OnSet = true;
            wheel.TurnOver = true;
            wheel.add_power += SpeedUP * 0.25f;
        }
        if (tc.TryGetComponent(out drill_hit DH))
        {
            DH.hit_value += HitUP * 2f;
        }
        if (tc.TryGetComponent(out ChainsawHit CSH))
        {
            CSH.hit_value += HitUP * 2f;
        }
        if (tc.TryGetComponent(out BroadswordMines bm))
        {
            bm.st = true;
            bm.Hit += HitUP * 0.5f;
            bm.Radius += RangeUP*0.5f;
        }
        if (tc.TryGetComponent(out IC_Code ic))
        {
            ic.setactive = true;
        }
        if (tc.transform.childCount > 0)
        {
            if (tc.transform.GetChild(0).TryGetComponent(out Artillery art))
            {
                art.Hit += HitUP;
                art.Radius+= RangeUP*0.5f;
            }
        }
    }

    private void OnValidate()
    {
        GetComponent<BoxCollider2D>().size = ColliderArea;
        GetComponent<SpriteRenderer>().size = ColliderArea;
    }
    bool On;
    private void OnEnable()
    {
        ClearCar();
        GetComponent<BoxCollider2D>().size = ColliderArea;
    }

    public Vector3 IndexToPos(int i)
    {
        return new Vector3((ColliderArea.x - 1f) * -0.5f + i % ColliderArea.x, (ColliderArea.y - 1f) * -0.5f + i / ColliderArea.x);
    }


    int HaveStart = 2;

    public bool SendCar;

    private void LateUpdate()
    {
        if (VideoSwarmPoint != null)
        {
            if (SendCar && !Sending)
            {
                StartCoroutine("SendCarNum", CarNum);
                SendCar = false;
            }
            return;
        }
        switch (GameManager.gameState)
        {
            case GameManager.GameState.Watch:
                if (Input.GetMouseButtonDown(0))
                {
                    foreach (Collider2D col in Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                    {
                        if (col.gameObject == gameObject)
                        {
                            lastTime = Time.time;
                        }
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    foreach (Collider2D col in Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                    {

                        if (Time.time - lastTime < 0.5f)
                        {
                            LC.all_cars .Clear();
                            LC.all_cars.Add(transform);
                            LC.AimPos = new Vector3(-2, -2, -2);
                            GameManager.gameState = GameManager.GameState.Ready;
                        }
                    }
                }
                break;
            case GameManager.GameState.Start:
                if (HaveStart > 0)
                {
                    if (HaveStart == 1)
                    {
                        sp.enabled = false;

                        CarStart();

                        Started = true;

                        GC.Collect();
                    }

                    HaveStart--;
                }
                break;
            case GameManager.GameState.Ready:
                if (lastLayIn != GameManager.layIn)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform ttf = transform.GetChild(i);
                        if (!GetT(ttf.gameObject, out TryConnect tc)) continue;
                        tc.LayInShow();
                    }
                }

                Vector2 Offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                HaveStart = 2;
                Vector2 Size = ColliderArea;
                if (Mathf.Abs(Offset.x) < Size.x / 2f && Mathf.Abs(Offset.y) < Size.y / 2f)
                {
                    Vector2 v2 = Size / 2f - new Vector2(0.5f, 0.5f) + Offset;
                    v2.x = Mathf.Round(v2.x);
                    v2.y = Mathf.Round(v2.y);
                    MouseOn = (int)(v2.x + v2.y * Size.x);
                }
                else
                {
                    MouseOn = -1;
                }
                if (StopTouch)//代码编辑中断
                {
                    Down = false;
                    return;
                }
                //点击移动逻辑
                if (Input.touchCount != 2)
                {
                    if (PutModeChange.Line)
                    {
                        if (Input.GetMouseButton(0) && !GameManager.DeleteMode)
                        {
                            LevelCamera.NonMove = true;
                            Click();
                        }
                        lastOn = -1;
                        Down = false;
                        lineMode = true;
                    }
                    else
                    {
                        if (lineMode)//记录撤销
                        {
                            MV_Manager.GetRecord();
                            lineMode = false;
                        }
                        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                        {
                            LevelCamera.NonMove = false;
                            if (Down)
                            {
                                if (Time.time - lastTime < 0.2f)
                                {
                                    if (CM < 2)
                                    {
                                        Click();
                                        MV_Manager.GetRecord();
                                    }
                                }
                                Down = false;
                            }
                        }
                        if (Input.GetMouseButtonDown(1) || (Input.GetMouseButtonDown(0) && GameManager.DeleteMode))
                        {
                            LevelCamera.NonMove = true;
                            lastRight = Time.time;
                            DeleteCD = false;

                        }
                       
                        if (Down)
                        {
                            CM += LevelCamera.CameraMove;
                            if (lastOn != MouseOn)
                            {
                                Move(lastOn);
                                Down = false;
                            }
                        }
                        if (MouseOn == -1) return;
                        if ((Input.GetMouseButton(1) || (Input.GetMouseButton(0) && GameManager.DeleteMode)) && !BlockManager.bm.IsOnUIElement(Input.mousePosition))//右键删除
                        {
                            if (Time.time - lastRight < 0.25f)
                            {
                                if (!DeleteCD)
                                {
                                    Delete();
                                    DeleteCD = true;
                                }
                            }
                            else
                            {
                                Delete();
                            }
                        }
                        if (Input.GetMouseButtonDown(0) && !BlockManager.bm.IsOnUIElement(Input.mousePosition))
                        {
                            if (!GameManager.DeleteMode)
                            {
                                lastTime = Time.time;
                                lastOn = MouseOn;
                                Down = true;
                                CM = 0;
                                if (AllObject[MouseOn].Count != 0)
                                {
                                    LevelCamera.NonMove = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Down = false;
                    lineMode = false;
                }
                break;
        }
    }
    int lastLayIn = -1;
    float lastRight;
    bool DeleteCD;
    bool lineMode;
    float CM;//cameraMove
    #region 这是一会要用到的妙妙小函数

    FixedJoint2D OwnRJset(TryConnect tc, TryConnect ty, bool collision, int f = 0, Transform AddOn = null)
    {
        FixedJoint2D rj = RJSet(tc, ty, collision, f, AddOn);
        CheckConnect(tc, ty, rj, f);
        return rj;
    }

    public static FixedJoint2D RJSet(TryConnect tc ,TryConnect ty ,bool collision,int f = 0,Transform AddOn = null)
    {
        FixedJoint2D rj;
        if (AddOn == null) rj= tc.gameObject.AddComponent<FixedJoint2D>();
        else rj = AddOn.gameObject.AddComponent<FixedJoint2D>();
        float strongenth = StrongthCalculate(tc.ConnectStrongth, ty.ConnectStrongth);
        rj.enableCollision = collision;
        rj.connectedBody = ty.rb_1;
        rj.breakForce = strongenth;
        rj.breakTorque = strongenth;

        //内部物体碰撞掉落无视
        if (f == 4) Physics2D.IgnoreCollision(tc.GetComponent<Collider2D>(), ty.GetComponent<Collider2D>());
        return rj;
    }

    public int GetIndexOfAllObject(Vector2 pos)
    {
        //得到相对坐标
        pos -= (Vector2)transform.position;
        int _index;
        Vector2 Size = ColliderArea;
        if (Mathf.Abs(pos.x) < Size.x / 2f && Mathf.Abs(pos.y) < Size.y / 2f)
        {
            Vector2 v2 = Size / 2f - new Vector2(0.5f, 0.5f) + pos;
            v2.x = Mathf.Round(v2.x);
            v2.y = Mathf.Round(v2.y);
            _index = (int)(v2.x + v2.y * Size.x);
        }
        else
        {
            _index = -1;
        }
        return _index;
    }

    void CheckConnect(TryConnect tc, TryConnect ty, FixedJoint2D rj,int f)
    {
        //CA连接
        tc.TryGetComponent(out CheckAlive CCA);
        ty.TryGetComponent(out CheckAlive YCA);
        if (CCA == null || YCA == null) return;
        CCA.allConnect.Add(rj);
        YCA.allConnect.Add(rj);
        CCA.CAList.Add(YCA);
        YCA.CAList.Add(CCA);
        CCA.BlockOffset.Add(f);
        if (f == 4) YCA.BlockOffset.Add(-4);
        else YCA.BlockOffset.Add(OP(f));
    }
    void Move(int where)
    {
        if (!FindLast(where, GameManager.layIn, out int Index, out TryConnect _)) return;
        SwarmBlock.BlockChoose = AllObject[where][Index];
        AllObject[where][Index].AddComponent<FollowMouse>();
        AllObject[where].RemoveAt(Index);
    }
    bool FindLast(int where,int layIn,out int Index,out TryConnect tc)
    {
        Index = -1;
        tc = null;
        if (AllObject[where].Count == 0) return false;
        for (int i = AllObject[where].Count - 1; i > -1; i--)
        {
            if (!GetT(AllObject[where][i], out tc)) continue;
            if (tc.layIn == layIn)
            {
                Index = i;
                return true;
            }
        }
        return false;
    }
    bool FindFirst(int where, int layIn, out int Index, out TryConnect tc)
    {
        Index = -1;
        tc = null;
        if (AllObject[where].Count == 0) return false;
        for (int i = 0; i < AllObject[where].Count; i++)
        {
            if (!GetT(AllObject[where][i], out tc)) continue;
            if (tc.layIn == layIn)
            {
                Index = i;
                return true;
            }
        }
        return false;
    }
    void Click()
    {
        if (MouseOn == -1) return;
        if (AllObject[MouseOn].Count != 0)
        {
            if (FindLast(MouseOn, GameManager.layIn, out int Index, out TryConnect tc))//get tc 判断是否要旋转
            {
                if (tc.Rotable)
                {
                    RotateItem(new(MouseOn, Index));
                }
            }
        }
        SwarmBlock.BlockChoose = gameObject;//不管怎么样先加入，之后加不进再退出
    }
    public static float StrongthCalculate(float A,float B)
    {
       return 2f * Mathf.Pow(A * B, 0.5f);
    }
    void Delete()
    {
        if (MouseOn == -1) return;
        if (!FindLast(MouseOn, GameManager.layIn, out int Index, out TryConnect _)) return;
        AllObject[MouseOn][Index].AddComponent<FollowMouse>().MoveAway();
        AllObject[MouseOn].RemoveAt(Index);
    }
    bool RemoveFromAllObject(int i, int ii, bool set)
    {
        AllObject[i].RemoveAt(ii);
        return set;
    }
    public bool FixedOn(int i,bool SetLayIn = true,bool DontDestroy = false)
    {
        int t = AllObject[i].Count - 1;
        if (t == -1) return false;
        if (!GetT(AllObject[i][t], out TryConnect tc)) return RemoveFromAllObject(i, t, false);
        if (SetLayIn) tc.layIn = GameManager.layIn;

        //先按layIn排序
        bool layOut;
        do
        {
            layOut = false;
            if(t == 0) break;
            if (!GetT(AllObject[i][t - 1], out TryConnect ty)) break;
            if (ty.layIn < tc.layIn)//交换
            {
                GameObject go = AllObject[i][t];
                AllObject[i].RemoveAt(t);
                AllObject[i].Insert(t - 1, go);
                t--;
                layOut = true;
            }
        } while (layOut);

        bool back = false;

        if (t != 0)//如果等于0 直接进入旋转
        {
            if (!GetT(AllObject[i][t - 1], out TryConnect ty)) return RemoveFromAllObject(i, t, false);
            if (ty.layIn == tc.layIn)
            {
                if (tc.Inside)
                {
                    if (ty.CantConnect[4]) back = true;//inside占位判断
                    if (tc.TryGetComponent(out Balloon _) && ty.TryGetComponent(out Balloon _))//气球容纳判断
                    {
                        back = false;
                        int count = 0;
                        for (int p = 0; p < t; p++)
                        {
                            if (AllObject[i][p].TryGetComponent(out Balloon _)) count++;
                        }
                        if (count > 2) back = true;//最多三个
                    }
                }
                else//尝试上位
                {
                    back = true;
                    if (!FindFirst(i, tc.layIn, out int Index, out ty)) return RemoveFromAllObject(i, t, false);
                    if (!tc.Rotable && tc.CantConnect[4] == ty.CantConnect[4] && !ty.Inside)//直接取代
                    {
                        //Video模式不要取代
                        if (DontDestroy) return RemoveFromAllObject(i, t, false);
                        if (tc != ty)
                        {
                            BlockManager.bm.GetBlock(AllObject[i][Index], 1);
                            Destroy(AllObject[i][Index]);
                            AllObject[i][Index] = AllObject[i][t];
                            AllObject[i].RemoveAt(t);
                        }
                        back = false;
                    }
                    else if (TryOut(i, t, out t)) back = false;
                }
                if (back)
                {
                    BlockManager.bm.GetBlock(AllObject[i][t], 1);
                    if (!DontDestroy) Destroy(AllObject[i][t]);
                    AllObject[i].RemoveAt(t);
                    return false;
                }
            }
            //接下来尝试旋转连接
        }
        if (tc.Rotable)
        {
            RotateItem(new(i, t),false);
        }
        //旋转完成后刷新临近的旋转
        for (int a = 0; a < 4; a++)
        {
            if (!FT(i, a, out int p)) continue;
            if (!FindFirst(p,tc.layIn,out int Index, out TryConnect ty)) continue;
            if (IsFixed(ty, new(p, Index))) continue;
            if (ty.Rotable) RotateItem(new(p, Index), false);
        }
        return !back;
    }
    bool TryOut(int x, int y,out int w)
    {
        w = y;
        if (y < 1) return true;
        if (!GetT(AllObject[x][y], out TryConnect tc)) return false;
        if (!GetT(AllObject[x][y - 1], out TryConnect ty)) return false;
        if (tc.layIn == ty.layIn && !tc.CantConnect[4] && ty.Inside)
        {
            GameObject go = AllObject[x][y];
            AllObject[x].RemoveAt(y);
            AllObject[x].Insert(y - 1, go);
            return TryOut(x, y - 1,out w);
        }
        else
        {
            return false;
        }
    }
    public static bool GetT(GameObject go, out TryConnect tc)
    {
        if (go != null)
        {
            if (!go.TryGetComponent(out tc))
            {
                if (go.transform.childCount > 0)
                {
                    tc = go.transform.GetChild(0).GetComponent<TryConnect>();
                }
            }
        }
        else
        {
            tc = null;
            return false;
        }
        return tc != null;
    }
    bool IsFixed(TryConnect tc, Vector2Int where)
    {
        if (!tc.Inside)
        {
            List<int> Connectable = new();
            bool[] after = CantAfterRotate(tc.CantConnect, tc.forward);
            for (int i = 0; i < 4; i++)//得到连接方向
            {
                if (!after[i])
                {
                    Connectable.Add(i);
                }
            }
            for (int i = 0; i < Connectable.Count; i++)
            {
                if (!FT(where.x, Connectable[i], out int p)) continue;
                if (!FindFirst(p,tc.layIn,out int _, out TryConnect ty)) continue;
                if (CantAfterRotate(ty.CantConnect, ty.forward)[OP(Connectable[i])]) continue;
                return true;
            }
        }
        return false;
    }
    private void RotateItem(Vector2Int where,bool RotateFirst = true)//旋转方向逆时针、直到连接、或者没有连接
    {
        if (AllObject[where.x].Count == 0) return; 
        GameObject go = AllObject[where.x][where.y];
        if (!GetT(go, out TryConnect tc)) return;
        if (!tc.Rotable) return;
        if (RotateFirst) RotateTo(where, NSZ(tc.forward));//第一步先转
        else RotateTo(where, tc.forward);
        bool connect = false;
        int limite = 0;
        bool[] after;
        List<int> Connectable = new();
        TryConnect ty;
        do
        {
            after = CantAfterRotate(tc.CantConnect, tc.forward);
            Connectable.Clear();
            for (int i = 0; i < 4; i++)//得到连接方向
            {
                if (!after[i])
                {
                    Connectable.Add(i);
                }
            }
            for (int i = 0; i < Connectable.Count; i++)
            {
                if (!FT(where.x, Connectable[i], out int p)) continue;
                if (AllObject[p].Count == 0) continue;
                if (!FindFirst(p,tc.layIn,out int _, out ty)) continue;
                if (CantAfterRotate(ty.CantConnect, ty.forward)[OP(Connectable[i])]) continue;
                connect = true;
            }
            limite++;
            if (!connect) RotateTo(where, NSZ(tc.forward));//再转
        } while (limite < 4 && !connect);
    }

    public void CarStart()
    {
        //添加加成
        foreach (Heath h in FindHeath(transform))
        {
            h.HP += HeathUP * 0.5f;
            h.max += HeathUP * 0.5f;
        }


        //连接所有内容
        TryConnect tc;
        TryConnect ty;
        for (int i = 0; i < AllObject.Count; i++)
        {
            if (AllObject[i].Count == 0) continue;


            for (int lay = 0; lay < 2; lay++)
            {
                if (!FindFirst(i, lay, out int Index, out tc)) continue;
                //固定内部物体
                for (int ii = Index + 1; ii < AllObject[i].Count; ii++)
                {
                    if (!GetT(AllObject[i][ii], out ty)) continue;//根据强度之和添加链接
                    if (ty.layIn == lay) OwnRJset(tc, ty, false, 4);
                }
                //如果是轴承则连接其他层的相邻部分
                var CAR = CantAfterRotate(tc.CantConnect, tc.forward);
                if (tc.LayerCross)
                {

                    int other_lay = lay switch { 0 => 1, 1 => 0 };

                    for (int p = 0; p < 4; p++)
                    {
                        if (CAR[p]) continue;
                        if (!FT(i, p, out int to)) continue;
                        if (!FindFirst(to, other_lay, out Index, out ty)) continue;
                        if (CantAfterRotate(ty.CantConnect, ty.forward)[OP(p)]) continue;
                        var rj = OwnRJset(tc, ty, false, p, tc.transform.GetChild(0));
                        rj.breakForce = float.PositiveInfinity;
                        rj.breakTorque = float.PositiveInfinity;
                    }
                }
                //连接相邻

                for (int p = 0; p < 4; p += 3)//只连接上右
                {
                    if (CAR[p]) continue;
                    if (!FT(i, p, out int to)) continue;
                    if (!FindFirst(to, lay, out Index, out ty)) continue;
                    if (CantAfterRotate(ty.CantConnect, ty.forward)[OP(p)]) continue;
                    OwnRJset(tc, ty, true, p);
                }
            }
        }

        List<Transform> tfl = new();
        Transform Rcenter = transform;

        List<IStart> reconnects = new();
        IStart[] reconnect;
        //对每个部件都执行
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform ttf = transform.GetChild(i);
            if (ttf.CompareTag("Center"))//核心相机绑定
            {
                if (VideoSwarmPoint == null)
                {
                    LC.all_cars.Add(ttf);
                }
                else
                {
                    VideoCameraFollow.CenterList.Add(ttf.GetComponent<Heath>());
                }
            }
            if (GetT(ttf.gameObject, out tc))//层级色彩重置
            {
                tc.BackColor();
            }
            ttf.GetComponent<CheckAlive>().SetStart(Time.time);//先提前启动CA
            reconnect = ttf.GetComponents<IStart>();
            if (reconnect != null) reconnects.AddRange(reconnect);

            if (GetT(ttf.gameObject, out tc))
            {
                ttf = tc.transform;
            }

            tfl.Add(ttf);
        }

        GameManager.Check();//检查所有
        reconnects.ForEach(s => s.SetStart(Time.time));//引擎、等组件检索启动
        foreach (TryConnect rb in FindTryConnect(transform))
        {
            rb.SetStart(Camp);
        }


        //移动所有
        if (VideoSwarmPoint != null)
        {
            for (int i = transform.childCount - 1; i > -1; i--)
            {
                Transform ttf = transform.GetChild(i);

                // 移动到位置

                ttf.parent = VideoSwarmPoint;
                Vector3 Offset = VideoSwarmPoint.position - transform.position;
                ttf.position += Offset;
                //也许不需要这个了吗？
                EnablePower(ttf);
            }
        }
    }

    int OP(int f)//对面
    {
        return f switch
        {
            0=>1,
            1=>0,
            2=>3,
            3=>2,
        };
    }
    bool FT(int i, int f, out int p)//面向的位置
    {
        p = i;
        switch (f)
        {
            case 0://up
                p += ColliderArea.x;
                break;
            case 1://down
                p -= ColliderArea.x;
                break;
            case 2://left
                p -= 1;
                break;
            case 3://right
                p += 1;
                break;
        }
        return !OnOtherSide(i, p);
    }
    public static bool[] CantAfterRotate(bool[] last, int f)//得到旋转后的连接Cant
    {
        bool[] final = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            final[SCT(i, f)] = last[i];
        }
        return final;
    }

    public static int SCT(int a, int f)
    {
        for (int i = 0; i < f switch {0=>0,1=>2,2=>1,3=>3 }; i++)
        {
            a = NSZ(a);
        }
        return a;
    }
    public static float AngleF(int forward)
    {
        return forward switch//旋转了多少度
        {
            0 => 0,
            2 => 90,
            1 => 180,
            3 => -90,
        };
    }

    public static int NSZ(int a)//逆时针映射
    { 
        return a switch//逆时针旋转90度
        {
            0 => 2,
            2 => 1,
            1 => 3,
            3 => 0,
            4 => 4,
        };
    }
    public static int SSZ(int a)//顺时针映射
    {
        return a switch//顺时针旋转90度
        {
            0 => 3,
            2 => 0,
            1 => 2,
            3 => 1,
            4 => 4,
        };
    }
    public void RotateTo(Vector2Int where, int f)
    {


        GameObject go = AllObject[where.x][where.y];
        if (!GetT(go, out TryConnect tc)) return;

        bool flip = f >= 10;
        if (flip)
        {
            f -= 10;
            if (go.TryGetComponent(out IFlip Flip))
            {
                Flip.flip = true;
            }
        }


        tc.forward = f;
        Transform tf = go.transform;
        switch(f)
        {
            case 0:
                tf.localEulerAngles = new(0,0,0);
                tf.localPosition = IndexToPos(where.x) + new Vector3(0, tc.addPercent - 1);
                break;
            case 1:
                tf.localEulerAngles = new(0, 0, 180);
                tf.localPosition = IndexToPos(where.x) - new Vector3(0, tc.addPercent - 1);
                break;
            case 2:
                tf.localEulerAngles = new(0, 0, 90);
                tf.localPosition = IndexToPos(where.x) - new Vector3(tc.addPercent - 1, 0);
                break;
            case 3:
                tf.localEulerAngles = new(0, 0, -90);
                tf.localPosition = IndexToPos(where.x) + new Vector3(tc.addPercent - 1, 0);
                break;
        }
    }

    public bool OnOtherSide(int origin, int towards)
    {
        Vector2 size = ColliderArea;
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


    #endregion
    public static bool StopTouch;
    bool Down;
    int lastOn = -1;
    public int MouseOn = -1;
    public bool Started;
    List<TryConnect> FindTryConnect(Transform father)
    {
        List<TryConnect> ah = new();
        if (father.TryGetComponent(out TryConnect h))
        {
            ah.Add(h);
        }
        for (int i = 0; i < father.childCount; i++)
        {
            ah.AddRange(FindTryConnect(father.GetChild(i)));
        }
        return ah;
    }
    List<Heath> FindHeath(Transform father)
    {
        List<Heath> ah = new();
        if (father.TryGetComponent(out Heath h))
        {
            ah.Add(h);
        }
        for (int i = 0; i < father.childCount; i++)
        {
            ah.AddRange(FindHeath(father.GetChild(i)));
        }
        return ah;
    }
    public List<string> origin_save = new();
    public List<string> load_save = new();


    public void ReloadCar()//复制
    {
        var carlist = BlockManager.bm.FindCar();

        //接下来调用BM中的生成
        BlockManager.bm.RefreshLevel(carlist.IndexOf(this), load_save);
    }

    public void RefreshCar()//初始化车辆
    {
        var carlist = BlockManager.bm.FindCar();

        //接下来调用BM中的生成
        BlockManager.bm.RefreshLevel(carlist.IndexOf(this), origin_save);
    }

    public void MoveCar(Vector3 move)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition += move;
        }
    }

    public void ClearCar()
    {
        AllObject.Clear();
        for (int i = 0; i < ColliderArea.x * ColliderArea.y; i++)
        {
            AllObject.Add(new List<GameObject> { });
        }
        sp.enabled = true;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public int ClearFloatBlock()
    {
        int count = 0;
        Transform P = transform.parent;
        for (int i = P.childCount - 1; i > 3; i--)
        {
            Destroy(P.GetChild(i).gameObject);
            count++;
        }
        return count;
    }
    void PutBlock(string Kind, Transform tf)
    {
        GameObject g = Resources.Load("GameObject/" + Kind) as GameObject;
        GameObject go = Instantiate(g);
        go.name = g.name;
        FollowMouse fm = go.AddComponent<FollowMouse>();
        // fm.AllHeath = h;
        fm.Moto(tf);
    }
}
