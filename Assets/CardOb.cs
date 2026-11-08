using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 实体卡，处理点击，动画
/// </summary>
public class CardOb : MonoBehaviour,IPointerDownHandler,IPointerExitHandler,IPointerUpHandler
{
    public Image Icon;
    public Text Name;
    public Text Cost;

    public CardState card_state;
    [BlockIndex]
    public int block_kind;//做在这里让Blockkind能被我方便的用coding_name选择
    public string card_name;//当为""尝试默认用BlockLibrary里的中文名
    public string describe;//当为空尝试默认用BlockLibrary里的**判词**。
    public string adjudication;//当为空尝试默认用BlockLibrary里的**判词**。
    public int cost;
    public int group_kind;//-1抽牌堆0手牌1弃牌堆
    public int group_index;
    public List<string> behaviour;//行动词条，当卡片类型为实物牌时激活优先生成对应的Block再运行这个。
    private FollowMouse block_item;//临时保存激活时生成的物体方便behaviour对它修改。

    public enum CardState
    { 
        实物牌,
        技能牌,
    }

    public void WriteIn(CardOb info)
    {
        this.card_state = info.card_state;
        this.block_kind = info.block_kind;
        this.cost = info.cost;
        this.group_kind = info.group_kind;
        this.group_index = info.group_index;
    }
    private void OnValidate()
    {
        Name.text = GameManager.BlockLibrary[block_kind].chinese_name;
        Cost.text = cost.ToString();
        Icon.sprite = GameManager.GetBlockSprite(block_kind);
        if (card_state == CardState.实物牌 && card_name == "") card_name = GameManager.GetBlockChineseName(block_kind);
        if (card_state == CardState.实物牌 && describe == "") describe = GameManager.GetBlockDescribe(block_kind);
        if (card_state == CardState.实物牌 && adjudication == "") adjudication = GameManager.GetBlockAdjudication(block_kind);
    }
    public void PreSwarm()//被鼠标按下拖动，生成物体，此时卡片变为一个半透明的虚空形态文本描述变为卡片区松开取消释放。
    { 
        
    }
    public void BeActive()//彻底激活
    { 
        
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        print("鼠标点击");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
