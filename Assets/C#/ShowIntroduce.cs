using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowIntroduce : MonoBehaviour
{
    private string[] context = new string[] { "引擎能使载具的速度和马力翻倍"
        , "速度越大，钻头撞击伤害越大"
        ,"激光对能量盾的伤害增加"
        , "激光是高贵的AOE"
        , "能量盾破了一段时间能恢复"
        , "轮子会转", "螺旋桨能飞"
        , "TNT会炸到自己人"
        , "修复器会优先修理生命少的单位"
        , "机枪有一定的击退效果"
        , "这一集做了有20多个小时了"
        , "看这些中文界面就知道不是搬运的了"
        ,"钻头和引擎的组合绝配"
        , "TNT：跟你们爆了！"
        , "射程、伤害对塔楼也有加成"
        , "摧毁车辆核心一切都灰飞烟灭"
        , "TNT能被射程加成"
        , "别忘了点赞"
        , "TNT血量为1e+11（1后面跟11个0）"
        , "前面忘了，中间忘了，后面忘了"
        , "一直走可以回到原点"
        , "所有的工作量只有我一个人做，累"
        , "装甲比一般框架重3倍"
        , "在生成载具时，小球会变成转盘点数"
        , "只有新造的车才会受到加成"
        , "真理只在大炮射程之内"
        , "飞行器的射程会增加"
        , "螺旋桨 + 钻头 = 航空史密斯"
        ,"一切终将消逝，只有bug永存"
        ,"TNT在飞机上会主动轰炸下方敌方"
        ,"没组装上的组件会重新转化为小球"
        ,"不会真的有人在盯着看吧"};
    private Text text;
    private void Start()
    {
        TryGetComponent(out text);
        StartCoroutine("Ciculation");
    }

    private List<int> left = new();
    IEnumerator Ciculation()
    {
        do 
        {
            yield return new WaitForSeconds(15f);
            if (left.Count == 0)
            {
                for (int t = 0; t < context.Length; t++)
                {
                    left.Add(t);
                }
            }
            int r = Random.Range(0, left.Count);
            text.text = context[left[r]];
            left.RemoveAt(r);
        } while (true);

    }
}
