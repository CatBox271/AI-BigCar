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
    public void OnPointerClick(PointerEventData eventData)
    {
        print("鼠标点击");
    }
}
