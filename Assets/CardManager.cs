using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public GameObject monocard;

    public static readonly List<CardOb> AllCardDefine = new();

    public List<CardOb> DrawPile = new();//≥È≈∆∂—
    public List<CardOb> Hand = new();// ÷≈∆
    public List<CardOb> DiscardPile = new();//∆˙≈∆∂—
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
