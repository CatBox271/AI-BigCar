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
            print(table.cards.Count);
            cardDefine.Clear();
            cardDefine.AddRange(table.cards);
        }
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

    // Start is called before the first frame update
    void Awake()
    {
        EnsureCardDefine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Start()
    {
        for (int i = 0; i < startPile.Count; i++)
        {
            AddCard(startPile[i]);
        }
    }
    public void AddCard(int cardindex,int whichPile = -1)
    {
        GameObject go = Instantiate(monocard, transform.parent);
        CardOb card = go.GetComponent<CardOb>();

        card.WriteIn(CardDefine[cardindex]);
        card.currentCamera = currentCamera;
        DrawPile.Add(card);
    }
}
