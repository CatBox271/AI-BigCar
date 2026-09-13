using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderContainer : MonoBehaviour
{
    public GameObject Container;
    public bool AnyBlock;
    public BlockInform BI;
    private void OnEnable()
    {
        lastRelative = RelativePos;
        if (AnyBlock)
        {
            Any();
        }
        Order();
    }

    public void Any()
    {
        for (int i = transform.childCount - 1; i > -1; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        for (int i = 1; i < GameManager.BlockIndex.Count; i++)
        {
            SwarmBlock sb = Instantiate(Container, transform).GetComponent<SwarmBlock>();
            sb.BlockKind = Resources.Load("GameObject/" + GameManager.BlockIndex[i]) as GameObject;
            sb.gameObject.SetActive(true);
            sb.Infinity = true;
            sb.Refresh();
        }
    }
    public void Add()
    {
        if (AnyBlock)
        {
            Any();
        }
        else
        {
            Instantiate(Container, transform);
        }
    }

    public Vector2 RelativePos;
    Vector2 lastRelative;

    int pass;
    private void Update()
    {
        if (lastRelative != RelativePos)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 v3 = RelativePos - lastRelative;
                transform.GetChild(i).localPosition += v3;
            }
            lastRelative = RelativePos;
        }

        pass++;
        if (pass < 120) return;
        pass = 0;
        Order();
    }

    void Order()
    {
        int Count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                transform.GetChild(i).localPosition = new Vector2(-350f + (Count / 2) * 100f, Count % 2 * -90f + 45f) + RelativePos;
                Count++;
            }
        }
    }
}
