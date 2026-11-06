using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IC_Code : MonoBehaviour,ION
{
    public string[] OutCode = new string[100];
    private sbyte[] Code = new sbyte[100];
    public GetPhysics GP;

    public ButtonInform BI;

    public List<(string, bool)> InputBool = new();
    public List<(string, float)> InputNum = new();

    public bool OnSet { get ; set; }
    public float NumSet { get; set; }

    List<sbyte> MemeryS = new();//正无穷true内存地址
    List<float> Memery = new();//负无穷false内存内容
    List<(string, float)> NumList = new();
    List<(string, float)> BoolList = new();

    List<sbyte> StringS = new();//位置
    List<string> StringN = new();//String

    List<sbyte> BracketStart = new();//{}括弧表//索引
    List<List<sbyte>> BracketContext = new();//{}括弧表//执行项

    List<sbyte> GotoS = new();//跳转表
    List<sbyte> GotoT = new();//跳转到

    public PrintSystem printSystem;

    sbyte runAt = 0;

    bool ForceGet = false;
    bool VideoMode = false;
    private void Start()
    {
        NewActual();
        actions = new Action[]
        {
            A0,
            A1,
            A2,
            A3,
            A4,
            A5,
            A6,
            A7,
            A8,
            A9,
            A10,
            A11,
            A12,
            A13,
            A14,
            A15,
            A16,
            A17,
            A18,
            A19,
            A20,
            A21,
            A22,
            A23,
            A24,
            A25,
            A26,
            A27,
            A28,
            A29,
            A30,
            A31,
            A32,
            A33,
            A34,
            A35,
            A36,
            A37,
            A38,
            A39,
            A40,
            A41,
            A42,
            A43,
            A44,
            A45,
            A46,
            A47,
            A48,
            A49,
            A50,
            A51,
            A52,
            A53,
            A54,
            A55,
            A56,
            A57,
            A58,
            A59,
            A60,
            A61,
            A62,
            A63,
            A64,
            A65,
            A66,
            A67,
            A68,
            A69,
            A70,
            A71,
            A72,
            A73,
            A74,
            A75
        };

        Obactions = new Action[]
        {
            B0,
            B1,
            B2,
            B3,
            B4,
            B5,
            B6,
            B7,
            B8,
        };
        if (Camera.main.name == "VideoMode")
        {
            VideoMode = true;
        }
    }


    List<string> NumKind = new();
    List<string> BoolKind = new();
    List<sbyte> Actual = new();
    List<sbyte> Pass = new();
   public  List<sbyte> OftenPass = new();
    bool first = true;

    public int Dnum;
    int dnum;

    public Collider2D _collider;
    float lt;
    public void ConnectItem()
    {
        BracketColorful.num = 0;
        List<GameObject> AllItem = new();
        Transform tf = transform.parent;
        Transform t;
        for (int i = 0; i < tf.childCount; i++)
        {
            t = tf.GetChild(i);
            if (t == transform) continue;
            AllItem.Add(t.gameObject);
        }
        List<(string, int)> CodeButton = new();
        List<(string, int)> SIL = new();//项目名称，旋转方向
        for (int i = 0; i < AllItem.Count; i++)//检索连接
        {
            //直接找父级要
            if (transform.parent.TryGetComponent(out OwnCar oc))
            {
                //不管接不接的上直接把所有的都分析一遍
                var all_object = oc.AllObject;
                //寻找buttonInform
                for (int b = 0; b < all_object.Count; b++)
                {
                    var ob = all_object[b];
                    for (int a = 0; a < ob.Count; a++)
                    {
                        if (ob[a].TryGetComponent(out ButtonInform BI))
                        {
                            int forward = ob[a].GetComponent<TryConnect>().forward;
                            for (int c = 0; c < BI.ButtonName.Count; c++)
                            {
                                SIL.Add((BI.ButtonName[c], forward));
                            }
                        }
                    }
                }
            }
        }
        //增加
        string s;
        for (int i = 0; i < 100; i++)
        {
            if (OutCode[i].StartsWith("Item:"))
            {
                s = OutCode[i].Replace("Item:", "");
                int k = s.LastIndexOf(":");
                SIL.Add((s.Substring(0, k), (int)float.Parse(s[(k + 1)..])));
            }
        }
        for (int i = 0; i < SIL.Count; i++)
        {
            //重复检查
            if (!CodeButton.Exists(s => s == SIL[i]))
            {
                CodeButton.Add(SIL[i]);
            }
        }
        BlockManager.bm.GetComponent<Animator>().Play("OpenCode");
        BlockManager.bm.CM.GetCode(OutCode, CodeButton, this);
        OwnCar.StopTouch = true;
    }
    //下面这段是抄的，能跑
    private EventSystem eventSystem;
    private PointerEventData eventData;
    private bool init = false;
    void Init()
    {
        if (!init)
        {
            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);
            init = true;
        }
    }
    public bool IsOnUIElement(Vector2 pos)
    {
        Init();
        eventData.pressPosition = pos;
        eventData.position = pos;
        List<RaycastResult> list = new();
        EventSystem.current.RaycastAll(eventData, list);


        foreach (var temp in list)
        {
            if (temp.gameObject.layer.Equals(5))
            {
                return true;//Equal(5)中的“5”是指图层第五层UI层
            }
        }
        return false;
    }
    //抄的到这里结束
    int start = 5;

    public bool setactive;
    private void FixedUpdate()
    {
        if (VideoMode && !setactive) return;
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            if (start > -1)
            {
                start--;
            }
            else
            {
                RunCode();
            }
        }
        else
        {
            if (TryGetComponent(out FollowMouse _)) return;
            if (Input.GetMouseButtonDown(0))
            {
                if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)) && !IsOnUIElement(Input.mousePosition))
                {
                    lt = Time.time;
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)) && !IsOnUIElement(Input.mousePosition))
                {
                    if (Time.time - lt > 1f)
                    {
                        ConnectItem();
                        lt = Time.time + 5f;
                    }
                }
                else
                {
                    lt = Time.time + 1f;
                }
            }
        }
    }
    void BADD(string n, int s, float cd, Vector3Int v3)
    {
        int k = BI.ButtonName.IndexOf(n);
        if (k == -1 || BI.ButtonState[k] != s)
        {
            BI.ButtonName.Add(n);
            BI.ButtonState.Add(s);
            BI.CD.Add(cd);
            BI.NumLimite.Add(v3);
        }
    }
    public void CreatList(bool avoid_repeat = true)
    {
        int _index;
        foreach (string c in OutCode)
        {
            _index = c.IndexOf("Input:Bool:");
            if (_index != -1)
            {
                InputBool.Add((c[(_index + 11)..], false));
                //
                BADD(c[(_index + 11)..], 0, 0, new());
                continue;
            }
            _index = c.IndexOf("Input:Num:");
            if (_index != -1)
            {
                InputNum.Add((c[(_index + 10)..], 0));
                //
                BADD(c[(_index + 10)..], 1, 0, new(0, 0, -1));
            }
        }
        CodeWarning(avoid_repeat);
    }

    public void CodeWarning(bool avoid_repeat = true)
    {
        string WarningContext = "<color=red>警告:当前无代码!</color>";
        List<string> check = new();
        check.AddRange(OutCode);
        if (check.TrueForAll(s => string.IsNullOrEmpty(s)))
        {
            if (!avoid_repeat || !printSystem.strTable.Contains(WarningContext)) printSystem.printStr(WarningContext);
        }
        else
        {
            printSystem.printStr("----载入代码成功----");
        }
    }
    void NewActual()
    {
        for (sbyte i = 0; i < 100; i++)
        {
            if (OutCode[i] == null) OutCode[i] = "";
        }
        first = true;
    }

    bool NextFrame;
    int runCount;
    public void RunCode()
    {
        if (!NextFrame)
        {
            for (int i = 0; i < Memery.Count; i++)
            {
                Memery[i] = float.NaN;
            }
        }
        if (first)
        {
            //第一次String码编译成数字码

            dnum = Dnum;
            Actual.Clear();
            OftenPass.Clear();
            for (sbyte i = 0; i < 100; i++)//具体编译
            {
                Compile(i);
            }
            int k; sbyte a;
            for (sbyte i = 0; i < Actual.Count; i++)
            {
                a = Actual[i];
                if (Code[a] > 35 && Code[a] < 44)//物理组件激活
                {
                    k = Code[a] switch
                    {
                        36 => 0,
                        37 => 1,
                        38 => 2,
                        39 => 2,
                        41 => 3,
                        42 => 3,
                        40 => 4,
                        43 => 5,
                    };
                    if (!GP.need.Contains(k)) GP.need.Add(k);
                }
            }
            first = false;
        }
        else
        {
            sbyte a;
            sbyte i_start = 0;
            forCount = 0;
            runCount = 0;
            if (NextFrame)
            {
                i_start = (sbyte)Actual.IndexOf(runAt);
                NextFrame = false;
            }
            else
            {
                Pass.Clear();
            }
            for (sbyte i = i_start; i < Actual.Count; i++)
            {
                a = Actual[i];
                if (Pass.Contains(a) || OftenPass.Contains(a)) continue;

                //运行runAt处的代码
                runAt = a;

                Run();
                if (NextFrame)
                {
                    break;
                }
            }
        }
    }

    public void Compile(sbyte i,string str = null,bool ACAdd = true)
    {
        if(str == null) str = OutCode[i];
        if (str.Length == 0)
        {
            Code[i] = -1;
            return;
        }
        int numof = accurate.IndexOf(str);
        if (numof != -1)
        {
            Code[i] = (sbyte)numof;
           if(ACAdd) Actual.Add(i);
        }
        else
        {
            numof = obscure.FindIndex(s => str.StartsWith(s));
            if (numof == -1)
            {
                Code[i] = -1;
                return;
            }
            Code[i] = (sbyte)-(numof + 2);
            if (ACAdd) Actual.Add(i);
            switch (numof)
            {
                case 0:
                    WSN(i, str[5..]);
                    break;
                case 1:
                    WSN(i, str[11..]);
                    break;
                case 2:
                    WSN(i, str[10..]);
                    break;
                case 3:
                    WSN(i, str[5..]);
                    break;
                case 4:
                    WSN(i, str[6..]);
                    break;
                case 5:
                    WSN(i, str[4..]);
                    break;
                case 6:
                    WSN(i, str[3..]);
                    break;
                case 7:
                    sbyte to = (sbyte)float.Parse(str.Substring(1, 2));
                    int XS, YS, XE, YE;
                    XS = i % 10;
                    XE = to % 10;
                    YS = i / 10;
                    YE = to / 10;
                    if (XE >= XS && YE >= YS)//先判断是否有效
                    {
                        BracketStart.Add(i);
                        List<sbyte> con = new();
                        sbyte s;
                        for (int y = YS; y <= YE; y++)
                        {
                            for (int x = XS; x <= XE; x++)
                            {
                                s = (sbyte)(x + y * 10);
                                OftenPass.Add(s);//这是让括弧没激活时运行时跳过
                                con.Add(s);//括弧表
                            }
                        }
                        OftenPass.Remove(i);//括弧首项不能ban
                        BracketContext.Add(con);//完成添加表
                    }
                    if (str.Length > 3)
                    {
                        Compile(i, str[3..], false);
                    }
                    break;
                case 8:
                    if (str.Length > 4)//goto
                    {
                        if (float.TryParse(str[5..], out float u))
                        {
                            GotoS.Add(i);
                            GotoT.Add((sbyte)u);
                        }
                    }
                    break;
            }
        }
    }


    public readonly List<string> obscure = new() {
        "Item:",//-2:0
        "Input:Bool:",
        "Input:Num:",//-4:2
        "List:",
        "Value:",//-6:4
        "Num:",
        "Bool:",//-8:6
        "{",
        "goto:",//-10:8
        "wait:"
    };
    //转换
    public readonly List<string> accurate = new() {
        "true",//0
        "false",
        "if",
        "else",//3
        "elif",
        "+",
        "+=",//6
        "-",
        "-=",
        "*",//9
        "*=",
        "/",
        "/=",//12
        "more",
        "less",
        "equal",//15
        "!",
        "&",
        "|",//18
        "^",
        "lerp",
        "pow",//21
        "log",
        "max",
        "min",//24
        "clamp",
        "sin",
        "cos",//27
        "tan",
        "asin",
        "acos",//30
        "atan",
        "abs",
        "floor",//33
        "round",
        "ceil",
        "g_s",//36
        "g_sa",
        "g_vx",
        "g_vy",//39
        "g_a",
        "g_vxa",
        "g_vya",//42
        "g_aa",
        "v_a",
        "for",//45
        "while_do",
        "do_while",
        "print",//48
        "p_x",
        "p_y",
        "start",//51
        "time",
    };

    #region Obaction
    Action[] Obactions;

    public CheckAlive ca;
    private void B0()
    {
        string v = RSN(runAt);
        if (v == null) return;
        float gn = GetNum(runAt, 2);
        if (!IsNum(gn)) gn = GetNum(runAt, 0);
        if (IsNum(gn)) GameManager.ButtonSet(ca.VID, v[0..^2], (int)float.Parse(v[^1..]), false, gn);
        gn = GetBool(runAt, 2);
        if (!IsBool(gn)) gn = GetBool(runAt, 0);
        if (IsBool(gn)) GameManager.ButtonSet(ca.VID, v[0..^2], (int)float.Parse(v[^1..]), float.IsPositiveInfinity(gn), float.NaN);
    }
    private void B1()
    {
        string v = RSN(runAt);
        if (v == null) return;
        bool bb = InputBool.Find(s => s.Item1 == v).Item2;
        Write(runAt, bb ? float.PositiveInfinity : float.NegativeInfinity);
        Assignment(runAt, bb ? float.PositiveInfinity : float.NegativeInfinity);
    }
    private void B2()
    {
        string n = RSN(runAt);
        if (n == null) return;
        float v = InputNum.Find(s => s.Item1 == n).Item2;
        Write(runAt, v);
        Assignment(runAt, v); 
    }
    private void B3()
    {

    }
    private void B4()
    {
        string n = RSN(runAt);
        if (n == null) return;
        if (float.TryParse(n, out float v))
        {
            Write(runAt, v);
            Assignment(runAt, v);
        }
    }
    private void B5()
    {
        float ram = Read(runAt);
        if (!IsNum(ram)) ram = NumRead(runAt);
        if (!IsNum(ram)) ram = 0;
        NumWrite(runAt, ram);
        Assignment(runAt, ram);
    }
    private void B6()
    {
        float ram = Read(runAt);
        if (!IsBool(ram)) ram = BoolRead(runAt);
        if (!IsBool(ram)) ram = float.NegativeInfinity;
        BoolWrite(runAt, ram);
        Assignment(runAt, ram);
    }
    private void B7()
    {

    }
    void B7_0(int t = -1)
    {
        if (t == -1) t = BracketStart.IndexOf(runAt);
        if (t == -1) return;
        var all = BracketContext[t];
        sbyte last = runAt;
        for (int i = 0; i < all.Count; i++)
        {
            ForceGet = true;
            runAt = all[i];

            if (Pass.Contains(runAt)) continue;

            int f = -1;
            if (BracketStart.Count > t + 1) f = BracketStart.FindIndex(t + 1, s => s == runAt);
            if (f != -1)
            {
                B7_0(f);
            }
            else
            {
                Run(last);
            }
        }
        ForceGet = false;
    }
    private void B8()//goto:
    {
        string n = RSN(runAt);
        if (n == null) return;
        if (float.TryParse(n, out float v))
        {
            if (v < 0 || v > 99) return;
            runAt = (sbyte)v;
        }
    }

    private void B9()
    {

    }

    private void B10()
    {

    }

    private void B11()
    {

    }

    private void B12()
    {

    }
    #endregion

    #region Action

    Action[] actions;
    private void A0()
    {
        Write(runAt, float.PositiveInfinity);
        Assignment(runAt, float.PositiveInfinity);
    }

    private void A1()
    {
        Write(runAt, float.NegativeInfinity);
        Assignment(runAt, float.NegativeInfinity);
    }

    private void A2()
    {
        float b = GetBool(runAt, 2);
        float b1 = GetBool(runAt, 0);
        if (float.IsNaN(b)) b = float.NegativeInfinity;
        if (float.IsNaN(b1)) b1 = float.NegativeInfinity;

        b = (float.IsPositiveInfinity(b) || float.IsPositiveInfinity(b1)) ? float.PositiveInfinity : float.NegativeInfinity;//两个输入都有时默认||
        Write(runAt, b);
        if (float.IsNegativeInfinity(b)) Ban(null, null, new() { 3, 4 });//ban,但忽略 3 else ，4 elif
    }
   
    private void A3()
    {
        float b = -IfFinder(runAt);//else
        Write(runAt, b);
        if (!float.IsPositiveInfinity(b)) Ban();//谁都ban
    }

    private void A4()
    {
        float b = -IfFinder(runAt);//elif
        print(b);
        if (!float.IsPositiveInfinity(b)) Ban(null, null, new() { 3, 4 });//如果false 或 NaN不继续执行
        A2();//执行if
    }

    private void A5()//+
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 0;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 0;
        float f = num1 + num2;
        Write(runAt, f);
        Assignment(runAt, f);
    }

   
    private void A6()//+=
    {
        AroundHave(new() { -7 }, runAt, out List<sbyte> ID);

        if (ID.Count == 0) return;

        float all = 0;
        for (sbyte i = 0; i < 4; i++)
        {
            if (i % 2 == 1) JustActive(runAt, i);//右和下
            float num1 = GetNum(runAt, i);
            if (!IsNum(num1)) num1 = 0;
            all += num1;
        }
        NumWrite(ID[0], all);
    }

    private void A7()//-
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 0;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 0;
        float f = num1 - num2;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A8()//-=
    {
        AroundHave(new() { -7 }, runAt, out List<sbyte> ID);

        if (ID.Count == 0) return;

        float all = NumRead(ID[0]);
        if (!IsNum(all)) all = 0;
        for (sbyte i = 0; i < 4; i++)
        {
            if (i % 2 == 1) JustActive(runAt, i);//右和下
            if (Index(runAt, i) == ID[0]) continue;
            float num1 = GetNum(runAt, i);
            if (!IsNum(num1)) num1 = 0;
            all -= num1;
        }
        NumWrite(ID[0], all);
    }

    private void A9()//*
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 1;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 1;
        float f = num1 * num2;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A10()//*=
    {
        AroundHave(new() { -7 }, runAt, out List<sbyte> ID);

        if (ID.Count == 0) return;

        float all = 0;

        for (sbyte i = 0; i < 4; i++)
        {
            if (i % 2 == 1) JustActive(runAt, i);//右和下
            float num1 = GetNum(runAt, i);
            if (!IsNum(num1)) num1 = 1;
            all *= num1;
        }
        NumWrite(ID[0], all);
    }

    private void A11()// /
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 1;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 1;
        if (num2 == 0) return;
        float f = num1 / num2;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A12()// /=
    {
        AroundHave(new() { -7 }, runAt, out List<sbyte> ID);

        if (ID.Count == 0) return;

        float all = NumRead(ID[0]);
        if (!IsNum(all)) all = 0;
        for (sbyte i = 0; i < 4; i++)
        {
            if (i % 2 == 1) JustActive(runAt, i);//右和下
            if (Index(runAt, i) == ID[0]) continue;
            float num1 = GetNum(runAt, i);
            if (!IsNum(num1)) num1 = 1;
            if (num1 == 0) return;
            all /= num1;
        }
        NumWrite(ID[0], all);
    }

    void JustActive(sbyte where,sbyte f)
    {
        sbyte k = (sbyte)Index(where, f);
        if (k == -1) return;
        if (Pass.Contains(k)) return;
        if (CanRightRead(Code[k]))
        {
            NotGet = true;
            sbyte l = runAt;
            runAt = k;
            Run();
            runAt = l;
            NotGet = false;
        }
    }

    private void A13()
    {
        List<float> all = new();
        bool RC = true;
        float num = GetNum(runAt, 0);
        if (IsNum(num)) all.Add(num);
        num = GetNum(runAt, 2);
        if (IsNum(num)) all.Add(num);

        JustActive(runAt, 3);
        num = GetNum(runAt, 3);
        if (IsNum(num))
        {
            all.Add(num);
            RC = false;
        }
        float final = float.NegativeInfinity;
        if (all.Count > 1)
        {
            final = float.PositiveInfinity;
            for (int ii = 1; ii < all.Count; ii++)
            {
                if (all[0] <= all[ii])
                {
                    final = float.NegativeInfinity;
                    break;
                }
            }
        }
        Write(runAt, final);
        Assignment(runAt, new sbyte[] { 1 }, final);
        if (RC) Assignment(runAt, new sbyte[] { 3 }, final);
    }

    private void A14()
    {
        List<float> all = new();
        bool RC = true;
        float num = GetNum(runAt, 0);
        if (IsNum(num)) all.Add(num);
        num = GetNum(runAt, 2);
        if (IsNum(num)) all.Add(num);
        JustActive(runAt, 3);
        num = GetNum(runAt, 3);
        if (IsNum(num))
        {
            all.Add(num);
            RC = false;
        }
        float final = float.NegativeInfinity;
        if (all.Count > 1)
        {
            final = float.PositiveInfinity;
            for (int ii = 1; ii < all.Count; ii++)
            {
                if (all[0] >= all[ii])
                {
                    final = float.NegativeInfinity;
                    break;
                }
            }
        }
        Write(runAt, final);
        Assignment(runAt, new sbyte[] { 1 }, final);
        if (RC) Assignment(runAt, new sbyte[] { 3 }, final);
    }

    private void A15()
    {
        List<float> all = new();
        bool RC = true;
        float num = GetNum(runAt, 0);
        if (IsNum(num)) all.Add(num);
        num = GetNum(runAt, 2);
        if (IsNum(num)) all.Add(num);

        JustActive(runAt, 3);
        num = GetNum(runAt, 3);
        if (IsNum(num))
        {
            all.Add(num);
            RC = false;
        }
        float final = float.NegativeInfinity;
        if (all.Count > 1)
        {
            final = float.PositiveInfinity;
            for (int ii = 1; ii < all.Count; ii++)
            {
                if (all[0] != all[ii])
                {
                    final = float.NegativeInfinity;
                    break;
                }
            }
        }
        Write(runAt, final);
        Assignment(runAt, new sbyte[] { 1 }, final);
        if (RC) Assignment(runAt, new sbyte[] { 3 }, final);
    }

    private void A16()
    {
        float b = GetBool(runAt, 2);
        if (!IsBool(b)) b = GetBool(runAt, 0);
        if (!IsBool(b)) return;
        b *= -1;
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A17()
    {
        float num1 = GetBool(runAt, 0);
        if (!IsBool(num1)) num1 = float.NegativeInfinity;
        float num2 = GetBool(runAt, 2);
        if (!IsBool(num2)) num2 = float.NegativeInfinity;
        float f = float.IsPositiveInfinity(num1) && float.IsPositiveInfinity(num2) ? float.PositiveInfinity : float.NegativeInfinity;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A18()
    {
        float num1 = GetBool(runAt, 0);
        if (!IsBool(num1)) num1 = float.NegativeInfinity;
        float num2 = GetBool(runAt, 2);
        if (!IsBool(num2)) num2 = float.NegativeInfinity;
        float f = float.IsPositiveInfinity(num1) || float.IsPositiveInfinity(num2) ? float.PositiveInfinity : float.NegativeInfinity;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A19()
    {
        float num1 = GetBool(runAt, 0);
        if (!IsBool(num1)) num1 = float.NegativeInfinity;
        float num2 = GetBool(runAt, 2);
        if (!IsBool(num2)) num2 = float.NegativeInfinity;
        float f = float.IsPositiveInfinity(num1) ^ float.IsPositiveInfinity(num2) ? float.PositiveInfinity : float.NegativeInfinity;
        Write(runAt, f);
        Assignment(runAt, f);
    }

    private void A20()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 0.5f;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 0;
        JustActive(runAt, 3);
       
        float num3 = GetNum(runAt, 3);
        if (!IsNum(num3)) num3 = 360;
        num1 = Mathf.Lerp(num2, num3, num1);
        Write(runAt, num1);
        NumWrite(Index(runAt, 1), num1);
    }

    private void A21()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) num1 = 2;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) num2 = 2;
        num1 = Mathf.Pow(num2, num1);

        Write(runAt, num1);
        Assignment(runAt, num1);
    }

    private void A22()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) return;
        else if (num1 <= 0) return;
        float num2 = GetNum(runAt, 0);
        if (!IsNum(num2)) return;
        else if (num2 <= 0) return;

        num1 = Mathf.Log(num2, num1);
        Write(runAt, num1);
        Assignment(runAt, num1);
    }

    private void A23()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) return;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) return;
        num1 = Mathf.Max(num2, num1);
        Write(runAt, num1);
        Assignment(runAt, num1);
    }

    private void A24()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) return;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) return;
        num1 = Mathf.Min(num2, num1);
        Write(runAt, num1);
        Assignment(runAt, num1);
    }

    private void A25()
    {
        float num1 = GetNum(runAt, 0);
        if (!IsNum(num1)) return;
        float num2 = GetNum(runAt, 2);
        if (!IsNum(num2)) return;

        JustActive(runAt, 3);
        
        float num3 = GetNum(runAt, 3);
        if (!IsNum(num3)) return;
        num1 = Mathf.Clamp(num1, num2, num3);
        Write(runAt, num1);
        Assignment(runAt, new sbyte[] { 1 }, num1);
    }

    private void A26()
    {
        float  b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Sin(b * Mathf.Deg2Rad);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A27()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Cos(b * Mathf.Deg2Rad);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A28()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Tan(b * Mathf.Deg2Rad);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A29()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Asin(b) * Mathf.Rad2Deg;
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A30()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Acos(b) * Mathf.Rad2Deg;
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A31()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Atan(b) * Mathf.Rad2Deg;
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A32()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Abs(b);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A33()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Floor(b);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A34()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Round(b);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A35()
    {
        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) return;
        b = Mathf.Ceil(b);
        Write(runAt, b);
        Assignment(runAt, b);
    }

    private void A36()
    {
        float s = GP.speed;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A37()
    {
        float s = GP.speedA;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A38()
    {
        float s = GP.vel.x;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A39()
    {
        float s = GP.vel.y;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A40()
    {
        float s = GP.ang;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A41()
    {
        float s = GP.velA.x;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A42()
    {
        float s = GP.velA.y;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A43()
    {
        float s = GP.angA;
        Write(runAt, s);
        Assignment(runAt, s);
    }

    private void A44()
    {
        float s = Vector3.Angle(Vector3.up, transform.up) * (transform.up.x > 0 ? 1 : -1);
        Write(runAt, s);
        Assignment(runAt, s);
    }
    List<int> ContinueOn = new();//各层进度 每次最多运行 100次。
    int layCount = 0;
    int forCount;
    private void A45()//for
    {
        int lay = layCount;
        layCount++;
        int i_start = 0;
        if (lay >= ContinueOn.Count) ContinueOn.Add(0);
        else i_start = ContinueOn[lay];

        float b = GetNum(runAt, 2);
        if (!IsNum(b)) b = GetNum(runAt, 0);
        if (!IsNum(b)) b = 0;

        if (b <= 0) return;
        sbyte lastRunAt = runAt;
        sbyte A = Index(lastRunAt, 3);
        sbyte B = Index(lastRunAt, 1);
        ForceGet = true;
        if (Code[A] == -1 && i_start < b) i_start += (int)b;
        else
            for (; i_start < b; i_start++)
            {
                if (forCount >= 200) break;
                runAt = A;
                Run();
                if (forCount >= 200) break;
                forCount++;
            }
        ForceGet = true;
        if (Code[B] == -1 && i_start >=b ) i_start += (int)b;
        else
            for (; i_start < b * 2; i_start++)
            {
                if (forCount >= 200) break;
                runAt = B;
                Run();
                if (forCount >= 200) break;
                forCount++;
            }
        ForceGet = false;
        runAt = lastRunAt;
        if (forCount >= 200)
        {
            ContinueOn[lay] = i_start;
            NextFrame = true;
        }
        else//结束
        {
            Ban();
            ContinueOn[lay] = 0;
        }
        layCount--;
    }

    private void A46()//while_do
    {

    }

    private void A47()//do_while
    {

    }

    private void A48()//print
    {
        float b = GetNum(runAt, 0);
        if (IsNum(b)) printSystem.PrintInfo(b);
        b = GetBool(runAt, 0);
        if (IsBool(b)) printSystem.PrintInfo(b);
        b = GetNum(runAt, 2);
        if (IsNum(b)) printSystem.PrintInfo(b);
        b = GetBool(runAt, 2);
        if (IsBool(b)) printSystem.PrintInfo(b);
    }

    private void A49()//x
    {
        float t = transform.position.x;
        Write(runAt, t);
        Assignment(runAt, t);
    }

    private void A50()
    {
        float t = transform.position.y;
        Write(runAt, t);
        Assignment(runAt, t);
    }
    bool start_pass;
    private void A51()
    {
        if (first) return;

        sbyte last = runAt;
        if (!start_pass)
        {
            ForceGet = true;
            runAt = Index(last, 1);
            if(runAt != -1) Run();
        }
        if (!NextFrame)
        {
            start_pass = true;
        }
        ForceGet = true;
        runAt = Index(last, 3);
        if (runAt != -1) Run();
        ForceGet = false;
        runAt = last;
        if (!NextFrame)
        {
            OftenPass.Add(runAt);
            BanOften();
            start_pass = false;
        }
    }

    private void A52()
    {

    }

    private void A53()
    {

    }

    private void A54()
    {

    }

    private void A55()
    {

    }

    private void A56()
    {

    }

    private void A57()
    {

    }

    private void A58()
    {

    }

    private void A59()
    {

    }

    private void A60()
    {

    }

    private void A61()
    {

    }

    private void A62()
    {

    }

    private void A63()
    {

    }

    private void A64()
    {

    }

    private void A65()
    {

    }

    private void A66()
    {

    }

    private void A67()
    {

    }

    private void A68()
    {

    }

    private void A69()
    {

    }

    private void A70()
    {

    }

    private void A71()
    {

    }

    private void A72()
    {

    }

    private void A73()
    {

    }

    private void A74()
    {

    }

    private void A75()
    {

    }

    #endregion

    #region 方法

    /// <summary>
    /// 指令集
    /// </summary>
    /// <param name="i">运行的位置</param>
    void Run(sbyte Bracket = -1)
    {
        if (!NotGet && Bracket != runAt)
        {
            if (BracketStart.Contains(runAt))
            {
                B7_0();
                return;
            }
        }
        if (Code[runAt] == -1) return;
        if(!NotGet && ForceGet) JustActive();
        if (Code[runAt] < -1) Obactions[-Code[runAt] - 2]();
        else actions[Code[runAt]]();
        runCount++;
    }

    float IfFinder(sbyte from)
    {
        float Out = float.NaN;
        bool SameLine;
        sbyte to;

        for (sbyte i = 1; i < 11; i += 9)
        {
            to = from;
            do
            {
                to -= i;//up
                SameLine = !OnOtherSide(from, to);
                if (!SameLine) break;
                if (Code[to] == 3) break;
                if (Code[to] == 2 || Code[to] == 4)//if 或 elif
                {
                    Out = Read(to);
                    if (float.IsPositiveInfinity(Out)) return Out;
                    break;
                }
            } while (SameLine);
        }
        return Out;
    }
    void JustActive(List<sbyte> s = null)
    {
        if (s == null) s = new() { 0, 2 };
        for (int i = 0; i < s.Count; i++)
        {
            JustActive(runAt, s[i]);
        }
    }

    readonly List<sbyte> CRR = new() {5, 7, 9, 11, 49, 50, 52 };
    bool CanRightRead(sbyte s)
    {
        return (-9 < s && s < -1 && s != -5) || (s > 19 && s < 45) || CRR.Contains(s);
    }
    float GetBool(sbyte i, sbyte kind)
    {
        sbyte idx = Index(i, kind);
        if (idx == -1) return float.NaN;
        float f = Read(idx);
        return float.IsInfinity(f) ? f : BoolRead(idx);
    }
    float GetNum(sbyte i, sbyte kind)
    {
        sbyte idx = Index(i, kind);
        if (idx == -1) return float.NaN;
        float f = Read(idx);
        return IsNum(f) ? f : NumRead(idx);
    }
    void Assignment(sbyte i, sbyte[] kind, float v)
    {
        if (NotGet) return;
        for (sbyte ii = 0; ii < kind.Length; ii++)
        {
            if (IsNum(v)) NumWrite(Index(i, kind[ii]), v);
            else BoolWrite(Index(i, kind[ii]), v);
        }
    }
    bool NotGet = false;



    void Assignment(sbyte i, float v)
    {
        if (NotGet) return;
        if (IsNum(v)) NumWrite(Index(i, 3), v);
        else BoolWrite(Index(i, 3), v);
        if (IsNum(v)) NumWrite(Index(i, 1), v);
        else BoolWrite(Index(i, 1), v);
    }
    float Read(sbyte i)
    {
        int k = MemeryS.FindIndex(s => s == i);
        if (k == -1) return float.NaN;
        return Memery[k];
    }
    void Write(sbyte i, float v)
    {
        int mi = MemeryS.FindIndex(s => s == i);
        if (mi == -1)
        {
            MemeryS.Add(i);
            Memery.Add(v);
        }
        else Memery[mi] = v;
    }
    void NumWrite(sbyte i, float v)
    {
        if (i == -1) return;
        if (!IsNum(v)) return;
        if (Code[i] == -7)
        {
            string st = RSN(i);
            if (st == null) return;
            int k = NumList.FindIndex(s => s.Item1 == st);
            if (k == -1) NumList.Add((st, v));
            else NumList[k] = (st, v);
        }
    }
    float NumRead(sbyte i)
    {
        if (i == -1) return float.NaN;
        if (Code[i] == -7)
        {
            string st = RSN(i);
            if (st == null) return float.NaN;
            int k = NumList.FindIndex(s => s.Item1 == st);
            if (k == -1) return float.NaN;
            return NumList[k].Item2;
        }
        return float.NaN;
    }
    void BoolWrite(sbyte i, float v)
    {
        if (i == -1) return;
        if (!IsBool(v)) return;
        if (Code[i] == -8)
        {
            string st = RSN(i);
            if (st == null) return;
            int k = BoolList.FindIndex(s => s.Item1 == st);
            if (k == -1) BoolList.Add((st, v));
            else BoolList[k] = (st, v);
        }
    }
    float BoolRead(sbyte i)
    {
        if (i == -1) return float.NaN;
        if (Code[i] == -8)
        {
            string st = RSN(i);
            if (st == null) return float.NaN;
            int k = BoolList.FindIndex(s => s.Item1 == st);
            if (k == -1) return float.NaN;
            return BoolList[k].Item2;
        }
        return float.NaN;
    }
    sbyte Opposite(sbyte kind)
    {
        return kind switch
        {
            0 => 1,
            1 => 0,
            2 => 3,
            3 => 2,
            _ => -1,
        };
    }
    sbyte Index(sbyte _index, sbyte kind)
    {
        sbyte aim = _index;
        switch (kind)
        {
            case 0://上
                aim -= 10;
                break;
            case 1:
                aim += 10;
                break;
            case 2:
                aim--;
                break;
            case 3:
                aim++;
                break;
        }
        if (OnOtherSide(_index, aim)) return -1;
        return aim;
    }
    sbyte Get(sbyte _index,sbyte kind)
    {
        sbyte aim = _index;
        switch (kind)
        {
            case 0://上
                aim -= 10;
                break;
            case 1:
                aim += 10;
                break;
            case 2:
                aim--;
                break;
            case 3:
                aim++;
                break;
        }
        if (OnOtherSide(_index, aim)) return -1;
        return Code[aim];
    }

    private bool OnOtherSide(sbyte origin, sbyte towards)
    {
        Vector2 size = new(10, 10);
        if (towards < 0 || towards > size.x * size.y - 1)
        {
            return true;
        }
        Vector2 PosA = new((sbyte)(origin / size.x), origin % size.x);
        Vector2 PosB = new((sbyte)(towards / size.x), towards % size.x);
        if (PosA.x == PosB.x || PosA.y == PosB.y)
        {
            return false;
        }
        return true;
    }

    #endregion

    bool IsNum(float f)
    {
        return float.IsNormal(f) || f == 0;
    }
    bool IsBool(float f)
    {
        return float.IsInfinity(f);
    }

    void WSN(sbyte i, string n)
    {
        int u = StringS.FindIndex(s => s == i);
        if (u == -1)
        {
            StringS.Add(i);
            StringN.Add(n);
        }
        else
        {
            StringN[u] = n;
        }
    }

    string RSN(sbyte i)
    {
        int u = StringS.FindIndex(s => s == i);
        if (u == -1)
        {
            return null;
        }
        else
        {
            return StringN[u];
        }
    }

    void AroundHave(List<sbyte> kind, sbyte where, out List<sbyte> ID)
    {
        ID = new();
        sbyte k;
        k = Get(where, 1);
        if (kind.Contains(k)) ID.Add(Index(where, 1));
        k = Get(where, 3);
        if (kind.Contains(k)) ID.Add(Index(where, 3));
        k = Get(where, 2);
        if (kind.Contains(k)) ID.Add(Index(where, 2));
        k = Get(where, 0);
        if (kind.Contains(k)) ID.Add(Index(where, 0));
    }

    /// <summary>
    /// 让被输入的地址失效一回
    /// </summary>
    /// <param name="where">地址</param>
    /// <param name="Positve">当它不为空时，包含代码才执行</param>
    /// <param name="Negitve">当它包含代码则不执行</param>
    void Ban(List<sbyte> where = null, List<sbyte> Positve = null, List<sbyte> Negitve = null)
    {
        if (where == null) where = new() { Index(runAt, 1), Index(runAt, 3) };//默认ban右和下
        if (Positve == null) Positve = new();
        if (Negitve == null) Negitve = new();
        sbyte k;
        for (int i = 0; i < where.Count; i++)
        {
            if (where[i] == -1) continue;
            k = Code[where[i]];
            if (!Negitve.Contains(k))
            {
                if (Positve.Count == 0 || Positve.Contains(k))
                {
                    Pass.Add(where[i]);
                    //现在要判断ban项是否会ban其他内容
                    if ((1 < k && k < 5) || (44 < k && k < 48))
                    {
                        Ban(new() { Index(where[i], 1), Index(where[i], 3) });
                    }
                }
            }
        }
    }
    void BanOften(List<sbyte> where = null, List<sbyte> Positve = null, List<sbyte> Negitve = null)
    {
        if (where == null) where = new() { Index(runAt, 1), Index(runAt, 3) };//默认ban右和下
        if (Positve == null) Positve = new();
        if (Negitve == null) Negitve = new();
        sbyte k;
        for (int i = 0; i < where.Count; i++)
        {
            if (where[i] == -1) continue;
            k = Code[where[i]];
            if (!Negitve.Contains(k))
            {
                if (Positve.Count == 0 || Positve.Contains(k))
                {
                    OftenPass.Add(where[i]);
                    //现在要判断ban项是否会ban其他内容
                    if ((1 < k && k < 5) || (44 < k && k < 48))
                    {
                        BanOften(new() { Index(where[i], 1), Index(where[i], 3) });
                    }
                }
            }
        }
    }
}
