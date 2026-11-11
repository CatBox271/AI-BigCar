using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    实物牌,
    技能牌,
}

[System.Serializable]
public class CardInfo
{
    public CardState card_state;
    public int block_kind;
    public string card_name;
    public string describe;
    public string adjudication;
    public int cost;
    public List<string> behaviour;
}

public class CardManager : MonoBehaviour
{
    public GameObject monocard;
    public Camera currentCamera;
    public CardCostBar costBar;//本方费用条，扣费和回费都走它
    public int handlimit = 8;
    public static List<CardInfo> CardDefine
    {
        get
        {
            EnsureCardDefine();
            return cardDefine;
        }
    }

    #region 读取卡片定义
    static void EnsureCardDefine()
    {
        if (cardDefine == null || cardDefine.Count == 0)
        {
            var asset = Resources.Load<TextAsset>(BlockLibraryPath);
            var table = JsonUtility.FromJson<CardDefineTable>(asset.text);
            cardDefine.Clear();
            cardDefine.AddRange(table.cards);
        }
    }

    public static void ClearCardDefine()//cards.json 重新导入时清空内存表
    {
        cardDefine.Clear();
    }

    public const string BlockLibraryPath = "Data/cards";
    [System.Serializable]
    public class CardDefineTable
    {
        public List<CardInfo> cards = new();
    }

    static readonly List<CardInfo> cardDefine = new();

    #endregion

    [CardIndex]
    public List<int> startPile = new();//初始放入抽牌堆的牌;

    public List<CardOb> DrawPile = new();//抽牌堆
    public List<CardOb> Hand = new();//手牌
    public List<CardOb> DiscardPile = new();//弃牌堆

    public float moveSpeed = 8f;//平滑速度
    public float catchInteval = 0.2f;
    private float _catch_interval;

    public List<Vector2> handPos = new();//每个手牌槽的位置，下标与 Hand 对齐

    // Start is called before the first frame update
    void Awake()
    {
        EnsureCardDefine();
    }

    // Update is called once per frame
    void Update()
    {
        if (_catch_interval > 0) _catch_interval -= Time.deltaTime;
        else 
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i] == null)
            {
                CatchCard(i);
                _catch_interval = catchInteval;
                break;
            }
        }
    }
    void BuildHandPos()//区域宽高等分：每行居中，卡多自动重叠，不超出区域
    {
        handPos.Clear();
        RectTransform area = (RectTransform)transform;
        float width = area.rect.width;
        float height = area.rect.height;
        float rowStep = handlimit > 1 ? height / 2 : 0;//两行中心距

        int top = (handlimit + 1) / 2;
        for (int i = 0; i < handlimit; i++)
        {
            int row = i < top ? 0 : 1;
            int count = row == 0 ? top : handlimit - top;
            int col = row == 0 ? i : i - top;
            float x = (col - (count - 1) / 2f) * (width / count);
            handPos.Add(new Vector2(x, row == 0 ? rowStep / 2 : -rowStep / 2));
        }
    }
    void RefreshPos()
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i] == null) continue;
            RectTransform rt = (RectTransform)Hand[i].transform;
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, handPos[i], Time.deltaTime * moveSpeed);
        }
    }
    private void Start()
    {
        for (int i = 0; i < startPile.Count; i++)
        {
            AddCard(startPile[i]);
        }
        for (int i = 0; i < handlimit; i++)
        {
            Hand.Add(null);
        }
        BuildHandPos();
        RefreshPos();
    }
    public CardOb CatchCard (int toIndex = -1, int toPile = 0, int fromPile = -1,int fromIndex = 0)
    {
        CardOb card = null;
        switch (fromPile)
        {
            case -1:
                //洗牌
                if (DrawPile.Count == 0) Shuffle();
                if (DrawPile.Count == 0) break;
                card = DrawPile[fromIndex];
                DrawPile.RemoveAt(fromIndex);
                break;
            case 0:
                card = Hand[fromIndex];
                Hand[fromIndex] = null;
                break;
            case 1:
                if (DiscardPile.Count == 0) break;
                card = DiscardPile[fromIndex];
                DiscardPile.RemoveAt(fromIndex);
                break;
        }
        if (card == null) return null;
        switch (toPile)
        {
            case -1:
                card.group_kind = -1;
                if (toIndex == -1) DrawPile.Add(card);
                else DrawPile.Insert(toIndex, card);
                break;
            case 0:
                card.group_kind = 0;
                if (toIndex != -1)
                {
                    card.group_index = toIndex;
                    Hand[toIndex] = card;
                }
                else
                {
                    int find = Hand.FindIndex(s => s == null);
                    if (find == -1)
                    {
                        card.group_index = 1;
                        DiscardPile.Add(card);
                    }
                    card.group_index = find;
                    Hand[find] = card;
                }
                break;
            case 1:
                card.group_kind = 1;
                if (toIndex == -1) DiscardPile.Add(card);
                else DiscardPile.Insert(toIndex, card);
                break;
        }
        SortHand();
        return card;
    }
    public void DiscardHand()//发车时手牌全部进弃牌堆
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i] == null) continue;
            Hand[i].group_kind = 1;
            DiscardPile.Add(Hand[i]);
            Hand[i] = null;
        }
        SortHand();
    }
    void SortHand()//按槽位重排，让重叠处的压盖顺序固定
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i] == null) continue;
            Hand[i].transform.SetSiblingIndex(i);
        }
    }
    public void Shuffle()//弃牌堆全部洗回抽牌堆
    {        DrawPile.AddRange(DiscardPile);        DiscardPile.Clear();        for (int i = DrawPile.Count - 1; i > 0; i--)        {            int r = Random.Range(0, i + 1);            (DrawPile[i], DrawPile[r]) = (DrawPile[r], DrawPile[i]);        }        DrawPile.ForEach(s => s.group_kind = -1);    }

    public void AddCard(int cardmodel, int whichPile = 1)
    {
        GameObject go = Instantiate(monocard, transform);
        go.transform.localPosition = new();
        CardOb card = go.GetComponent<CardOb>();

        card.WriteIn(CardDefine[cardmodel]);
        card.currentCamera = currentCamera;
        card.group_kind = whichPile;
        switch (whichPile)
        {
            case -1:
                DrawPile.Add(card);
                break;
            case 0:

                break;
            case 1:
                DiscardPile.Add(card);
                break;
        }
    }
}
