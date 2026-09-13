using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 物体名字和数字对照表
    /// </summary>
    public static readonly List<string> BlockIndex = new()
    {
        "Null",//0
        "Artillery",
        "Block",
        "BroadswordMines",//3
        "Center",
        "drill",
        "Engine",//6
        "FixingBag",
        "Groscope",
        "Laser",//9
        "panzer",
        "PowerPropellers",
        "PowerWheel",//12
        "RPG",
        "shield",
        "Stealth",//15
        "TNT",
        "BrakeWheel",
        "Umbrella",//18
        "Spring",
        "PowerStickWheel",
        "PowerUmbrella",//21
        "HalfBlock",
        "NoColliderBlock",
        "PowerSmallPropellers",//24
        "Wing",
        "Tail",
        "H-Balloon",//27
        "He-Balloon",
        "GearBox",
        "HydraulicRod",//30
        "PowerWheelBig",
        "PowerWheelHuge",
        "PowerFan",//33
        "HydraulicHinge",
        "CompressionTank",
        "AntiwaterBlock",//36
        "FloatingBlock",
        "PowerWaterPropellers",
        "PowerWaterSmallPropellers",//39
        "PowerWaterWheel",
        "GeneralCannon",
        "Battery",//42
        "NuclearBattery",
        "Generator",
        "CIWS",//45
        "IC",
        "TurbinEngine",
        "H-Block",//48
        "He-Block",
        "Sail",
        "Chainsaw",//51
        "Flamethrower",
        "WoodBlock",
        "Cola",
        "Sprite",
        "PowerHinge",
    };

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

    public static void Check(int VID = -114514)
    {
        if (VID == -114514)//检查所有
        {
            for (int i = 0; i < allvehicle.Count; i++)
            {
                SingleCheck(i);
            }
        }
        else
        {
            SingleCheck(VID);
        }
    }
    public static List<List<ButtonInfo>> allInfo = new();

    static void SingleCheck(int id)//button消息系统
    {
        KeepSameCount();

        allInfo[id].Clear();//清空旧数据

        List<CheckAlive> allItem = new();
        allItem.AddRange(allvehicle[id]);//对所有通一载具内容进行分析
        for (int i = 0; i < allItem.Count; i++)
        {
            if (allItem[i] == null) continue;
            var GBI = allItem[i].GetComponent<ButtonInform>();//要生成的Button信息
            if (GBI == null) continue;
            for (int ii = 0; ii < GBI.ButtonName.Count; ii++)
            {
                TryConnect tc = GBI.GetComponent<TryConnect>();
                if (tc == null) continue;

                if (ButtonFind(id, GBI.ButtonName[ii], tc.forward, out int AddTo))//已经有过了
                {
                    allInfo[id][AddTo]._all_on.Add(GBI.GetComponent<ION>());
                }
                else
                {
                    ButtonInfo BI = new();
                    ION Ion = GBI.GetComponent<ION>();
                    if (Ion == null) continue;
                    bool StartOn = Ion.OnSet;
                    float StartNum = Ion.NumSet;
                    if (GBI.TryGetComponent(out IC_Code ic))
                    {
                        BI.Set(GBI.ButtonName[ii], -1, GBI.ButtonState[ii], GBI.CD[ii], GBI.NumLimite[ii], new() { Ion }, StartOn, StartNum, ic);//写入全部信息
                    }
                    else BI.Set(GBI.ButtonName[ii], tc.forward, GBI.ButtonState[ii], GBI.CD[ii], GBI.NumLimite[ii], new() { Ion }, StartOn, StartNum);//写入全部信息
                    allInfo[id].Add(BI);
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
