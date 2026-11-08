using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 实体卡，处理点击，动画
/// </summary>
public class CardOb : MonoBehaviour, IPointerClickHandler
{
    public Image Icon;
    public Text Name;
    public Text Cost;

    public CardState card_state;
    [BlockIndex]
    public int block_kind;//做在这里让Blockkind能被我方便的用coding_name选择
    public int cost;
    public int group_kind;//-1抽牌堆0手牌1弃牌堆
    public int group_index;

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
        Icon.sprite = null;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        print("鼠标点击");
    }
}
