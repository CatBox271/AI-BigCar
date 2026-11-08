using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region 方块管理
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
    public static readonly List<BlockInfo> BlockLibrary = new()
    {
        new("Null", "空气", "什么也没有", "看不见摸不着"),
        new("Artillery", "自动炮台", "<color=#FFFF80FF>作用</color>：自动索敌，锁定最近的敌人后开火\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 0.5，连接强度 1.05\n<color=orange>特性</color>：射程 18，不耗电，没被打坏就会一直打", "射程之内皆真理"),
        new("Block", "铁框", "<color=#FFFF80FF>作用</color>：连接四周，中心可以放置“引擎”、“气球”、“雀斑猪”等类似组件\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 1，连接强度 1800", "车辆的基石"),
        new("BroadswordMines", "阔剑地雷", "<color=#FFFF80FF>作用</color>：一次性的定向霰弹\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 0.75，连接强度 1.05,总伤害 450", "此面向敌"),
        new("Center", "核心", "<color=#FFFF80FF>作用</color>：整台载具的核心，所有按钮都由它下发\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 1，连接强度 1.05\n<color=orange>特性</color>：核心被摧毁，整车直接散架", "整车的心脏"),
        new("drill", "钻头", "<color=#FFFF80FF>作用</color>：近战撞击\n<color=#FFCB31FF>属性</color>：基础生命 250，质量 1.5，连接强度 2.8 ，每秒接触伤害 225\n<color=orange>特性</color>：撞击时会额外造成伤害，相对速度越大，伤害越大。", "近战"),
        new("Engine", "引擎", "<color=#FFFF80FF>作用</color>：增强动力组件，如轮子、螺旋桨、风扇。\n<color=#FFCB31FF>属性</color>：基础生命 100，质量 2，连接强度 1.05\n<color=orange>特性</color>：进水会坏熄火，只能放置在方框里", "环保少女趋势器"),
        new("FixingBag", "修复器", "<color=#FFFF80FF>作用</color>：定时修复周围受伤最重的友方单位\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 0.75，连接强度 1.05\n<color=orange>特性</color>：修复逻辑已经写好，预制体上还没挂上", "先救最惨的那个"),
        new("Groscope", "陀螺仪", "<color=#FFFF80FF>作用</color>：开启后锁住全车部件的旋转\n<color=#FFCB31FF>属性</color>：基础生命 25，质量 5，连接强度 1.05\n<color=orange>特性</color>：可以当开关用；锁住时车不会翻跟头，轮子不受影响", "扶稳了别翻"),
        new("Laser", "激光炮", "<color=#FFFF80FF>作用</color>：直线命中，不需要提前量的激光\n<color=#FFCB31FF>属性</color>：基础生命 30，质量 0.5，连接强度 1.05\n<color=orange>特性</color>：射程 19，单发 15 伤害，对能量盾伤害加成", "高贵的 AOE"),
        new("panzer", "铸铁装甲", "<color=#FFFF80FF>作用</color>：连接四周，防御\n<color=#FFCB31FF>属性</color>：基础生命 1200，质量 20，连接强度 50\n<color=orange>特性</color>：当受到的伤害小于750，只会造成30点伤害；当超过时，伤害为30+超过的量", "纯粹的强大"),
        new("PowerPropellers", "螺旋桨", "<color=#FFFF80FF>作用</color>：驱动载具飞行\n<color=#FFCB31FF>属性</color>：基础生命 30，质量 1.2，连接强度 3.2\n<color=orange>特性</color>：正朝上时全力推进，倾斜超过阈值后改为沿自身朝向推；受引擎加成", "起飞"),
        new("PowerWheel", "驱动轮", "<color=#FFFF80FF>作用</color>：驱动车辆行驶\n<color=#FFCB31FF>属性</color>：基础生命 70，质量 1，连接强度 3", "看好了，滚动摩擦是这样用的"),
        new("RPG", "火箭筒", "<color=#FFFF80FF>作用</color>：自动索敌，朝最近的敌人发射火箭弹\n<color=#FFCB31FF>属性</color>：基础生命 40，质量 20，连接强度 1.05\n<color=orange>特性</color>：冷却 15 秒，一发中心伤害 10、爆炸范围 7.5；看不见隐身的单位", "一发入魂"),
        new("shield", "能量盾", "<color=#FFFF80FF>作用</color>：张开一层能吸收伤害的能量盾\n<color=#FFCB31FF>属性</color>：基础生命 5，质量 1.75，连接强度 1.05\n<color=orange>特性</color>：盾值 1200 打空后破防 10 秒，之后自动恢复", "破了还能长回来"),
        new("Stealth", "隐身装置", "<color=#FFFF80FF>作用</color>：让整台载具从敌方视野里消失\n<color=#FFCB31FF>属性</color>：基础生命 25，质量 2，连接强度 1.05\n<color=orange>特性</color>：每秒耗电 0.5，最多撑 40 秒", "你看不见我"),
        new("TNT", "TNT", "<color=#FFFF80FF>作用</color>：带有冲击波的范围伤害。可以被敌方破坏 或 主动引爆\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 1.6，连接强度 1.8，中心伤害 10 ，爆炸范围 7.5\n<color=orange>特性</color>：既可以放置在方框外，也可以放置在方框里", "飞起来！"),
        new("BrakeWheel", "刹车轮", "<color=#FFFF80FF>作用</color>：初始状态无动力，启动后将刹车\n<color=#FFCB31FF>属性</color>：基础生命 70，质量 1，连接强度 3", "摩擦，摩擦，在光滑的地面上摩擦"),
        new("Umbrella", "减速伞", "<color=#FFFF80FF>作用</color>：展开伞面，拖慢载具的速度\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.25，连接强度 2\n<color=orange>特性</color>：伞面越开阻力越大，速度太快时伞会被吹坏", "慢一点，再慢一点"),
        new("Spring", "弹簧", "<color=#FFFF80FF>作用</color>：连接两端物体，在外力作用下产生弹性形变，并吸收一定的能量。弹力和形变成正比，但过大的形变下，弹力大小会达到极限\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 1，连接强度 ∞ 。", "胡克定律是这样用的"),
        new("PowerStickWheel", "吸盘轮", "<color=#FFFF80FF>作用</color>：驱动车辆行驶，并吸附一切接触到的物体\n<color=#FFCB31FF>属性</color>：基础生命 70，质量 1，连接强度 3", "是你吗？彼得帕克"),
        new("PowerUmbrella", "动力伞", "<color=#FFFF80FF>作用</color>：自动开合，靠扇动伞面提供额外推力\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 0.5，连接强度 1.05\n<color=orange>特性</color>：开合频率受引擎加成，推力也跟着涨", "扇一扇就上天"),
        new("HalfBlock", "半框", "<color=#FFFF80FF>作用</color>：只有正常框架一半大小的连接件\n<color=#FFCB31FF>属性</color>：基础生命 25，质量 0.5，连接强度 1.8\n<color=orange>特性</color>：重量和生命都只有铁框的一半，适合拿来补缝", "半截也是块"),
        new("NoColliderBlock", "无实体框", "<color=#FFFF80FF>作用</color>：连接四周，中心可以放置“引擎”、“气球”、“雀斑猪”等类似组件\n<color=#FFCB31FF>属性</color>：基础生命 40，质量 0.5，连接强度 0.95\n<color=orange>特性</color>：不会与任何物体发生碰撞", "不在同一图层"),
        new("PowerSmallPropellers", "小型螺旋桨", "<color=#FFFF80FF>作用</color>：轻量的推进器\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.4，连接强度 2.4\n<color=orange>特性</color>：力道比不上螺旋桨，好处是又轻又小", "小而有力"),
        new("Wing", "机翼", "<color=#FFFF80FF>作用</color>：靠前进速度产生升力\n<color=#FFCB31FF>属性</color>：基础生命 15，质量 1，连接强度 2.4\n<color=orange>特性</color>：升力和阻力都随速度增加；准备阶段点一下可以翻转方向", "速度换升力"),
        new("Tail", "尾翼", "<color=#FFFF80FF>作用</color>：给载具加一点俯仰姿态\n<color=#FFCB31FF>属性</color>：基础生命 15，质量 1，连接强度 3\n<color=orange>特性</color>：角度可以调，正数抬头，负数低头", "管姿态的"),
        new("H-Balloon", "氢气球", "<color=#FFFF80FF>作用</color>：提供向上的浮力\n<color=#FFCB31FF>属性</color>：基础生命 3，质量 0.33，连接强度 1.05\n<color=orange>特性</color>：被打爆会炸，威力 20、范围 10，小心自己人", "会飞的炸弹"),
        new("He-Balloon", "氦气球", "<color=#FFFF80FF>作用</color>：提供向上的浮力\n<color=#FFCB31FF>属性</color>：基础生命 3，质量 0.33，连接强度 1.05\n<color=orange>特性</color>：被打爆也不会炸，比氢气球安全", "安全的浮力"),
        new("GearBox", "变速箱", "<color=#FFFF80FF>作用</color>：把一路动力分给多组齿轮，并改变传递方向\n<color=#FFCB31FF>属性</color>：基础生命 15，质量 1，连接强度 1.05\n<color=orange>特性</color>：档位从 -1 到 5 可调，能倒转也能分配动力", "动力管理大师"),
        new("HydraulicRod", "液压杆", "<color=#FFFF80FF>作用</color>：连接两端，在限定行程内伸缩\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 1.5，连接强度 1.05\n<color=orange>特性</color>：伸缩有上下限，超出范围的形变会被卡住", "能屈能伸"),
        new("PowerWheelBig", "大型驱动轮", "<color=#FFFF80FF>作用</color>：大号驱动轮，驱动车辆行驶\n<color=#FFCB31FF>属性</color>：基础生命 125，质量 1.5，连接强度 3\n<color=orange>特性</color>：轮径更大，过坑能力更强，代价是更重", "大脚"),
        new("PowerWheelHuge", "巨型驱动轮", "<color=#FFFF80FF>作用</color>：超大号驱动轮\n<color=#FFCB31FF>属性</color>：基础生命 450，质量 5，连接强度 3\n<color=orange>特性</color>：又重又硬，翻山越岭专用", "翻山越岭"),
        new("PowerFan", "风扇", "<color=#FFFF80FF>作用</color>：可以驱动车辆，但是力道不及小型螺旋桨\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.45，连接强度 2.4\n<color=orange>特性</color>：对着地面吹风时，推动效果会大大增加", "地效飞行器"),
        new("HydraulicHinge", "液压铰链", "<color=#FFFF80FF>作用</color>：可转动的铰链，用来做折叠结构\n<color=#FFCB31FF>属性</color>：基础生命 100，质量 11，连接强度 4.8\n<color=orange>特性</color>：转动部分是独立刚体，很重，小心重心", "会折叠的关节"),
        new("CompressionTank", "压缩气罐", "<color=#FFFF80FF>作用</color>：把附近气球里的气抽进罐里存起来\n<color=#FFCB31FF>属性</color>：基础生命 30，质量 2，连接强度 1.05\n<color=orange>特性</color>：罐子容量 120，存气降浮，放气升空", "存气降浮"),
        new("AntiwaterBlock", "防水框", "<color=#FFFF80FF>作用</color>：连接四周，中心可以放置“引擎”、“气球”、“雀斑猪”等类似组件\n<color=#FFCB31FF>属性</color>：基础生命 100，质量 1.25，连接强度 3.6\n<color=orange>特性</color>：放置于其中的引擎不会因为进水而损坏", "潜水必备"),
        new("FloatingBlock", "泡沫塑料", "<color=#FFFF80FF>作用</color>：连接四周，浮在水上\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.01，连接强度 0.1", "软又轻"),
        new("PowerWaterPropellers", "水中螺旋桨", "<color=#FFFF80FF>作用</color>：水中推进\n<color=#FFCB31FF>属性</color>：基础生命 60，质量 2.5，连接强度 3.2\n<color=orange>特性</color>：入水后推力大幅增强，陆上几乎使不上劲", "水下全速"),
        new("PowerWaterSmallPropellers", "小型水中螺旋桨", "<color=#FFFF80FF>作用</color>：轻量的水中推进器\n<color=#FFCB31FF>属性</color>：基础生命 45，质量 1，连接强度 2.4\n<color=orange>特性</color>：入水后推力增强，适合水下微调姿态", "水下微调"),
        new("PowerWaterWheel", "水陆两栖轮", "<color=#FFFF80FF>作用</color>：驱动车辆行驶，能在水中前进\n<color=#FFCB31FF>属性</color>：基础生命 70，质量 1，连接强度 3", "全地形"),
        new("GeneralCannon", "通用火炮", "<color=#FFFF80FF>作用</color>：手动开火的炮弹发射器\n<color=#FFCB31FF>属性</color>：基础生命 5，质量 15，连接强度 1.05\n<color=orange>特性</color>：炮弹伤害高，开火的反作用力会把载具往后推", "开炮要付后坐力"),
        new("Battery", "电池", "<color=#FFFF80FF>作用</color>：给耗电设备供电\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 2，连接强度 1.05\n<color=orange>特性</color>：容量 500，左侧的条会显示剩余电量", "有电才有盾"),
        new("NuclearBattery", "核电池", "<color=#FFFF80FF>作用</color>：给耗电设备供电，火力全开\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 2，连接强度 1.05\n<color=orange>特性</color>：容量 500，产量是普通电池的十倍", "电管够"),
        new("Generator", "发电机", "<color=#FFFF80FF>作用</color>：给全车的电池充电，并联动引擎\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 2，连接强度 1.05\n<color=orange>特性</color>：可以当总电源开关用", "总电源"),
        new("CIWS", "近防炮", "<color=#FFFF80FF>作用</color>：以极高的射速持续扫射最近的敌人\n<color=#FFCB31FF>属性</color>：基础生命 240，质量 10，连接强度 4.5\n<color=orange>特性</color>：射程 27.5，连飞来的炮弹都会被它当成目标", "弹幕洗地"),
        new("IC", "可编程芯片", "<color=#FFFF80FF>作用</color>：接管载具的逻辑控制，可以写自定义按钮\n<color=#FFCB31FF>属性</color>：基础生命 25，质量 0.1，连接强度 1.05\n<color=orange>特性</color>：几乎不占重量，但芯片本身也怕打", "写代码的地方"),
        new("TurbinEngine", "涡轮引擎", "<color=#FFFF80FF>作用</color>：喷出气流直接推动载具\n<color=#FFCB31FF>属性</color>：基础生命 75，质量 7.5，连接强度 3\n<color=orange>特性</color>：推力要慢慢爬升，关机后也会慢慢停", "慢慢加速"),
        new("H-Block", "氢气框", "<color=#FFFF80FF>作用</color>：既是框架又能提供浮力\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 1，连接强度 1.5\n<color=orange>特性</color>：被打爆会炸，等于把炸弹绑在车上", "浮力带自爆"),
        new("He-Block", "氦气框", "<color=#FFFF80FF>作用</color>：既是框架又能提供浮力\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 1，连接强度 1.5\n<color=orange>特性</color>：被打爆也不会炸，安全可靠", "浮力不带炸"),
        new("Sail", "帆", "<color=#FFFF80FF>作用</color>：张开帆面，提供水平推力\n<color=#FFCB31FF>属性</color>：基础生命 50，质量 1，连接强度 1.05\n<color=orange>特性</color>：帆面立得越直推力越大，方向可以反转", "看风向"),
        new("Chainsaw", "电锯", "<color=#FFFF80FF>作用</color>：贴在敌人身上持续切割\n<color=#FFCB31FF>属性</color>：基础生命 300，质量 0.5，连接强度 1.05\n<color=orange>特性</color>：血量极高，接触即持续掉血", "贴着就不松口"),
        new("Flamethrower", "火焰喷射器", "<color=#FFFF80FF>作用</color>：喷出火焰的范围武器\n<color=#FFCB31FF>属性</color>：基础生命 30，质量 0.5，连接强度 1.05\n<color=orange>特性</color>：射程只有 8，单次伤害低但能持续灼烧", "近身烧烤"),
        new("WoodBlock", "木框", "<color=#FFFF80FF>作用</color>：轻量的木质框架\n<color=#FFCB31FF>属性</color>：基础生命 20，质量 0.1，连接强度 0.9\n<color=orange>特性</color>：全场最轻的框架之一，一撞就散", "轻，但脆"),
        new("Cola", "可乐", "<color=#FFFF80FF>作用</color>：暂无实装效果\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.25，连接强度 0.9\n<color=orange>特性</color>：按钮和减重都留好了，功能还空着", "还没开瓶"),
        new("Sprite", "雪碧", "<color=#FFFF80FF>作用</color>：暂无实装效果\n<color=#FFCB31FF>属性</color>：基础生命 10，质量 0.25，连接强度 0.9\n<color=orange>特性</color>：按钮和减重都留好了，功能还空着", "还没开瓶"),
        new("PowerHinge", "动力铰链", "<color=#FFFF80FF>作用</color>：预留的动力铰链\n<color=#FFCB31FF>属性</color>：暂无数据\n<color=orange>特性</color>：预制体还没做出来", "还在图纸上"),
    };

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
    #endregion;

    #region 贴图管理
    public static Sprite GetResourcesSprite(string path)//通用获取方法
    { 
        
    }
    public static Sprite GetBlockSprite(int index)
    { 
        
    }
    public static Sprite GetBlockSprite(string code_name)//用code_name的名字
    {

    }
    public static Sprite GetBlockSprite(string chinese_name)//用code_name的名字
    {

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
