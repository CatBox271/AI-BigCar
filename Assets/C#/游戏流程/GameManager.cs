using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region 方块管理
    [System.Serializable]
    public class BlockInfo
    {
        public string code_name;//代号
        public string chinese_name;//中文名称
        public string describe;//详细描述
        public string adjudication;//判词，幽默的简单印象
        public BlockInfo() { }
        public BlockInfo(string _code_name, string _chinese_name, string _describe,string _adjudication)
        {
            code_name = _code_name;
            chinese_name = _chinese_name;
            describe = _describe;
            adjudication = _adjudication;
        }
    }

    [System.Serializable]
    public class BlockLibraryTable
    {
        public List<BlockInfo> blocks = new();
    }

    public const string BlockLibraryPath = "Data/BlockLibrary";

    // 表体内存。对外统一走 BlockLibrary 属性，保证第一次访问时就已从 JSON 载入。
    static readonly List<BlockInfo> blockLibrary = new();

    public static List<BlockInfo> BlockLibrary
    {
        get
        {
            EnsureBlockLibrary();
            return blockLibrary;
        }
    }

    static void EnsureBlockLibrary()
    {
        if (blockLibrary.Count > 0) return;
        var table = SLManager.ImportFromResources<BlockLibraryTable>(BlockLibraryPath);
        blockLibrary.Clear();
        blockLibrary.AddRange(table.blocks);
    }

    public static void ClearBlockLibrary()//BlockLibrary.json 重新导入时清空内存表
    {
        blockLibrary.Clear();
    }

    public static string GetBlockCodeName(int index)
    {
        if (index > -1 && index < BlockLibrary.Count)
        {
            return BlockLibrary[index].code_name;
        }
        else return "";
    }
    public static string GetBlockCodeName(string chinese_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.chinese_name == chinese_name)
            {
                return item.code_name;
            }
        }
        return "";
    }
    public static string GetBlockChineseName(int index)
    {
        if (index > -1 && index < BlockLibrary.Count)
        {
            return BlockLibrary[index].chinese_name;
        }
        else return "";
    }
    public static string GetBlockChineseName(string code_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.code_name == code_name)
            {
                return item.chinese_name;
            }
        }
        return "";
    }

    public static int GetBlockIndexByChinese(string chinese_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.chinese_name == chinese_name)
            {
                return i;
            }
        }
        return -1;
    }
    public static int GetBlockIndexByCode(string code_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.code_name == code_name)
            {
                return i;
            }
        }
        return -1;
    }
    public static string GetBlockDescribe(string code_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.code_name == code_name)
            {
                return item.describe;
            }
        }
        return "";

    }
    public static string GetBlockDescribe(int index)
    {
        if (index > -1 && index < BlockLibrary.Count)
        {
            return BlockLibrary[index].describe;
        }
        else return "";
    }
    public static string GetBlockAdjudication(string code_name)
    {
        for (int i = 0; i < BlockLibrary.Count; i++)
        {
            var item = BlockLibrary[i];
            if (item.code_name == code_name)
            {
                return item.adjudication;
            }
        }
        return "";
    }
    public static string GetBlockAdjudication(int index)
    {
        if (index > -1 && index < BlockLibrary.Count)
        {
            return BlockLibrary[index].adjudication;
        }
        else return "";
    }

    #endregion;

    #region 代码百科
    [System.Serializable]
    public class CodeInfo
    {
        public string kind;//指令字符串，和 IC_Code.accurate / obscure 里的写法一致
        public string describe;//详细描述
    }

    [System.Serializable]
    public class CodeLibraryTable
    {
        public List<CodeInfo> codes = new();
    }

    public const string CodeLibraryPath = "Data/CodeLibrary";

    // 表体内存。对外统一走 CodeLibrary 属性，保证第一次访问时就已从 JSON 载入。
    static readonly List<CodeInfo> codeLibrary = new();

    public static List<CodeInfo> CodeLibrary
    {
        get
        {
            EnsureCodeLibrary();
            return codeLibrary;
        }
    }

    static void EnsureCodeLibrary()
    {
        if (codeLibrary.Count > 0) return;
        var table = SLManager.ImportFromResources<CodeLibraryTable>(CodeLibraryPath);
        codeLibrary.Clear();
        codeLibrary.AddRange(table.codes);
    }

    public static void ClearCodeLibrary()//CodeLibrary.json 重新导入时清空内存表
    {
        codeLibrary.Clear();
    }

    public static string GetCodeDescribe(string kind)
    {
        if (string.IsNullOrEmpty(kind)) return "";
        for (int i = 0; i < CodeLibrary.Count; i++)
        {
            if (CodeLibrary[i].kind == kind) return CodeLibrary[i].describe;
        }
        //带名字的指令（Item:xx / Input:Bool:xx / Num:xx 之类）退回前缀那一条
        for (int i = 0; i < CodeLibrary.Count; i++)
        {
            if (kind.StartsWith(CodeLibrary[i].kind)) return CodeLibrary[i].describe;
        }
        return "";
    }
    #endregion

    #region 贴图管理
    static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite> blockSpriteCache = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
    static Dictionary<string, string> spriteIndex;

    /// <summary>通用获取方法：按完整 Resources 路径取 Sprite。带缓存，查不到也会缓存 null。</summary>
    public static Sprite GetResourcesSprite(string path)//通用获取方法
    {
        if (string.IsNullOrEmpty(path)) return null;
        Sprite sp;
        if (spriteCache.TryGetValue(path, out sp)) return sp;
        sp = Resources.Load<Sprite>(path);
        spriteCache[path] = sp;
        return sp;
    }

    /// <summary>按下标取道具图标，下标与 BlockLibrary / 存档里的数字索引一致。</summary>
    public static Sprite GetBlockSprite(int index)
    {
        return GetBlockSprite(GetBlockCodeName(index));
    }

    /// <summary>按 code_name 取道具图标。自动去掉 "(Clone)" 之类后缀，并兼容历史命名的大小写差异。</summary>
    public static Sprite GetBlockSprite(string code_name)//用code_name的名字
    {
        if (string.IsNullOrEmpty(code_name)) return null;
        int cut = code_name.IndexOf('(');
        if (cut > 0) code_name = code_name.Substring(0, cut).TrimEnd();

        Sprite sp;
        if (blockSpriteCache.TryGetValue(code_name, out sp)) return sp;

        sp = GetResourcesSprite("Sprite/" + code_name);
        if (sp == null)
        {
            string actual = FindSpriteNameIgnoreCase(code_name);
            if (actual != null) sp = GetResourcesSprite("Sprite/" + actual);
        }
        blockSpriteCache[code_name] = sp;
        return sp;
    }

    /// <summary>图标历史命名大小写不统一（engine / Panzer / H-BLock / Shield），这里只查真实文件名，加载仍交给 GetResourcesSprite。</summary>
    static string FindSpriteNameIgnoreCase(string code_name)
    {
        if (spriteIndex == null)
        {
            spriteIndex = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var s in Resources.LoadAll<Sprite>("Sprite"))
            {
                if (s == null || spriteIndex.ContainsKey(s.name)) continue;
                spriteIndex[s.name] = s.name;
            }
        }
        string hit;
        return spriteIndex.TryGetValue(code_name, out hit) ? hit : null;
    }

    /// <summary>清空图标缓存。编辑器里重新生成图标后调用一次即可。</summary>
    public static void ClearSpriteCache()
    {
        spriteCache.Clear();
        blockSpriteCache.Clear();
        spriteIndex = null;
    }
    #endregion

    public static GameState gameState = GameState.Watch;
    public enum GameState
    {
        Ready,
        Watch,
        Anim,
        Start,
        Win,
        TurnWin,
        Fail,
    }

    public static int layIn = 0;

    public static bool DeleteMode = false;

    public static List<List<CheckAlive>> allvehicle = new();//所有载具

    public static void Check(List<int> vids = null)
    {
        if (vids == null)//检查所有
        {
            for (int i = 0; i < allvehicle.Count; i++)
            {
                SingleCheck(i);
            }
        }
        else foreach(int id in vids)
        {
            SingleCheck(id);
        }
    }
    public static List<List<ButtonInfo>> allInfo = new();

    static readonly Dictionary<(string, int), int> buttonIndex = new();//本车里已有按钮的位置，替掉反复 FindIndex 的字符串扫描
    static void SingleCheck(int id)//button消息系统
    {
        KeepSameCount();

        List<ButtonInfo> info = allInfo[id];
        info.Clear();//清空旧数据
        buttonIndex.Clear();

        List<CheckAlive> allItem = allvehicle[id];//直接遍历，不再每次复制一份
        for (int i = 0; i < allItem.Count; i++)
        {
            CheckAlive ca = allItem[i];
            if (ca == null) continue;
            if (ca.VID != id) continue;//槽里残留的、已经属于别的载具的引用，跳过
            //TryGetComponent：方块大多没有 ButtonInform，走 GetComponent 每次拿 null 都要 Unity 生成异常消息字符串（压力测试里单帧 59ms）
            if (!ca.TryGetComponent(out ButtonInform GBI)) continue;//要生成的Button信息
            for (int ii = 0; ii < GBI.ButtonName.Count; ii++)
            {
                if (!GBI.TryGetComponent(out TryConnect tc)) continue;

                var key = (GBI.ButtonName[ii], tc.forward);
                if (buttonIndex.TryGetValue(key, out int AddTo))//已经有过了
                {
                    if (GBI.TryGetComponent(out ION ionAdd)) info[AddTo]._all_on.Add(ionAdd);
                    if (GBI.TryGetComponent(out IC_Code icAdd))//同名按钮被多个芯片声明时，每个芯片都要收到输入
                    {
                        ButtonInfo share = info[AddTo];
                        if (share.IC == null) share.IC = new();
                        if (!share.IC.Contains(icAdd)) share.IC.Add(icAdd);
                        info[AddTo] = share;
                    }
                }
                else
                {
                    if (!GBI.TryGetComponent(out ION Ion)) continue;
                    ButtonInfo BI = new();
                    bool StartOn = Ion.OnSet;
                    float StartNum = Ion.NumSet;
                    if (GBI.TryGetComponent(out IC_Code ic))
                    {
                        BI.Set(GBI.ButtonName[ii], -1, GBI.ButtonState[ii], GBI.CD[ii], GBI.NumLimite[ii], new() { Ion }, StartOn, StartNum, ic);//写入全部信息
                    }
                    else BI.Set(GBI.ButtonName[ii], tc.forward, GBI.ButtonState[ii], GBI.CD[ii], GBI.NumLimite[ii], new() { Ion }, StartOn, StartNum);//写入全部信息
                    buttonIndex[key] = info.Count;
                    info.Add(BI);
                }
            }
        }
    }
    static bool ButtonFind(int VID, string _name,int f,out int Index)
    {
        Index = allInfo[VID].FindIndex(bi => bi._name == _name && bi._forward == f);
        return Index != -1;
    }
    public struct ButtonInfo
    {
        public string _name;
        public int _forward;
        public int _state;
        public float _cd;
        public Vector3Int _limit;
        public List<ION> _all_on;
        public bool _set_on;
        public float _set_num;
        public float LockTime;//CD要冷却到
        public List<IC_Code> IC;
        public void Set(string I_name,int forward,int I_state,float I_cd,Vector3Int I_limit,List<ION> I_all_on,bool I_set_on,float I_set_num,IC_Code IIC = null)
        {
            _name = I_name;
            _forward = forward;
            _state = I_state;
            _cd = I_cd;
            _limit = I_limit;
            _all_on = I_all_on;
            _set_on = I_set_on;
            _set_num = I_set_num;
            if (IIC != null)
            {
                if (IC == null)
                {
                    IC = new();
                }
                IC.Add(IIC);
            }
        }
    }
    static void KeepSameCount()
    {
        for (int i = allInfo.Count; i < allvehicle.Count; i++)
        {
            allInfo.Add(new());
        }
    }
    public static int ButtonGet(int VID, string button_name,int f, out float Get)//Positive =True,Negative = False
    {
        Get = float.NaN;
        if (!ButtonFind(VID, button_name,f, out int aim)) return -1;
        ButtonInfo BI = allInfo[VID][aim];
        var L_ON = BI._all_on.Find(s => s != null);
        if (L_ON == null) return -1;
        switch (BI._state)
        {
            case 0://bool 
                Get = L_ON.OnSet ? float.PositiveInfinity : float.NegativeInfinity;
                return BI._state;
            case 1://num
                Get = L_ON.NumSet;
                return BI._state;
        }
        return -1;
    }
    public static void ButtonSet(int VID,string button_name, int f, bool BTO,float NTO)
    {
        if (!ButtonFind(VID, button_name,f, out int aim)) return;
        ButtonInfo BI = allInfo[VID][aim];
        if (BI.LockTime > Time.time) return;
        bool isNumWrite = !float.IsNaN(NTO);//开关写一律传 NaN，数字写传实数
        if (BI._state == 0 && isNumWrite) return;//数字写进开关组件时不做处理，别把它当成关掉
        if (BI._state == 1 && !isNumWrite) return;//开关写进数字组件同理，不会写成 NaN
        var L_ON = BI._all_on;
        switch (BI._state)
        {
            case 0://bool 
                L_ON.ForEach(ob =>
                {
                    if (ob != null)
                    {
                        if (BI.IC != null)
                        {
                            BI.IC.ForEach(ic =>
                            {
                                ic.InputBool[ic.InputBool.FindIndex(s => s.Item1 == BI._name)] = (BI._name, BTO);
                            });
                        }
                        else
                        {
                            ob.OnSet = BTO;
                            BI.LockTime = Time.time + BI._cd;
                        }
                    }
                });
                break;
            case 1://num
                L_ON.ForEach(ob => {
                    if (ob != null)
                    {
                        if (BI.IC != null)
                        {
                            BI.IC.ForEach(ic =>
                            {
                                ic.InputNum[ic.InputNum.FindIndex(s => s.Item1 == BI._name)] = (BI._name, NTO);
                            });
                        }
                        else
                        {
                            ob.NumSet = NTO;
                            BI.LockTime = Time.time + BI._cd;
                        }
                    }
                });
                break;
        }
        allInfo[VID][aim] = BI;
    }
}
