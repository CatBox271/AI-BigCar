using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 实体卡，处理点击，动画
/// </summary>
public class CardOb : MonoBehaviour, IPointerDownHandler
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
    public Camera currentCamera;

    private Color card_color = Color.white;//拖拽前的卡面颜色
    private string card_text = "";//拖拽前的卡名文本
    private bool dragging;//正在拖拽生成中

    public void WriteIn(CardInfo info)
    {
        card_state = info.card_state;
        block_kind = info.block_kind;
        cost = info.cost;
        card_name = info.card_name;
        describe = info.describe;
        adjudication = info.adjudication;
        behaviour.Clear();
        behaviour.AddRange(info.behaviour);
        Refresh();
    }
    private void Refresh()
    {
        Name.text = GameManager.BlockLibrary[block_kind].chinese_name;
        Cost.text = cost.ToString();
        Icon.sprite = GameManager.GetBlockSprite(block_kind);
        if (card_state == CardState.实物牌 && card_name == "") card_name = GameManager.GetBlockChineseName(block_kind);
        if (card_state == CardState.实物牌 && describe == "") describe = GameManager.GetBlockDescribe(block_kind);
        if (card_state == CardState.实物牌 && adjudication == "") adjudication = GameManager.GetBlockAdjudication(block_kind);
    }
    private void OnValidate()
    {
        Refresh();
    }
    public void PreSwarm()//被鼠标按下拖动，生成物体，此时卡片变为一个半透明的虚空形态文本描述变为卡片区松开取消释放。
    {
        if (dragging || block_item != null) return;
        if (card_state != CardState.实物牌) return;
        string code = GameManager.GetBlockCodeName(block_kind);
        if (string.IsNullOrEmpty(code) || code == "Null") return;
        GameObject prefab = Resources.Load<GameObject>("GameObject/" + code);
        if (prefab == null)
        {
            Debug.LogWarning("CardOb：找不到预制体 Resources/GameObject/" + code);
            return;
        }

        GameObject go = Instantiate(prefab);
        go.name = prefab.name;
        go.SetActive(true);

        Camera cam = currentCamera;
        if (cam != null)
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            go.transform.position = pos;
        }

        FollowMouse fm = go.GetComponent<FollowMouse>();
        if (fm == null) fm = go.AddComponent<FollowMouse>();
        fm.FromCard = this;
        fm.Cam = cam;

        block_item = fm;
        BeginDragVisual();
    }
    public void BeActive()//彻底激活：卡片打出后从手中移除
    {
        dragging = false;
        block_item = null;
        gameObject.SetActive(false);
    }

    public void OnPlaced(FollowMouse fm)//物体成功落位到载具上，卡片消失
    {
        if (!ReferenceEquals(block_item, fm)) return;//用引用判等，避开 Unity 销毁后的伪 null
        BeActive();
    }
    public void OnPlaceCancel(FollowMouse fm)//松手位置不在载具上，物体销毁，卡片还原
    {
        if (ReferenceEquals(block_item, fm)) block_item = null;
        EndDragVisual();
    }
    void BeginDragVisual()
    {
        dragging = true;
        Image img = GetComponent<Image>();
        if (img != null)
        {
            card_color = img.color;
            img.color = new Color(card_color.r, card_color.g, card_color.b, 0.35f);
        }
        if (Name != null)
        {
            card_text = Name.text;
            Name.text = "松开取消";
        }
    }
    void EndDragVisual()
    {
        if (!dragging) return;
        dragging = false;
        Image img = GetComponent<Image>();
        if (img != null) img.color = card_color;
        if (Name != null) Name.text = card_text;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        PreSwarm();
    }
}
