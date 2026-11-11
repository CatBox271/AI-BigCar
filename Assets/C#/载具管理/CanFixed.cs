using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanFixed : MonoBehaviour
{
    public Rigidbody2D rb;
    private Heath h;
    private float HP;
    public Artillery at;

    void Start()
    {
        TryGetComponent(out h);

        h.ND();
        HP = h.HP;
    }

    bool Break;

    private int lastLayer = int.MinValue;

    // 改 Layer 时连子物体一起改：碰撞与索敌都按 Layer 判定，
    // 只改根物体会让自带 Collider 的子物体留在旧层上。
    // lastLayer 记住上次下发的层号，Update 每帧调用时不重复遍历子树。
    private void SetLayerAll(int layer)
    {
        if (layer == lastLayer && gameObject.layer == layer) return;
        lastLayer = layer;
        SetLayerAll(gameObject, layer);
    }

    private static void SetLayerAll(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            SetLayerAll(t.GetChild(i).gameObject, layer);
        }
    }

    private void Update()
    {
        if (h.HP <= 0)
        {
            if (!Break)
            {
                SetLayerAll(6);
                h.camp = 0;
                at.enabled = true;
                HP *= 1.25f;
                at.Hit *= 1.5f;
                Break = true;
                at.enabled = false;
            }
            if (h.HP < -HP)
            {
                h.HP = HP;
                h.max = HP;
                if (h.KeepRecord[1] > h.KeepRecord[2])
                {
                    h.Set(1);
                    SetLayerAll(8);
                }
                else
                {
                    h.Set(2);
                    SetLayerAll(7);
                }
                h.KeepRecord = new() { 0, 0, 0 };
                h.ND();
                at.camp = h.camp;
            }
        }
        else
        {
            if (h.camp == 0) return;
            if (Break)
            {
                Break = false;
            }
            at.enabled = true;
            // 中立 6 / 左 7（camp 2 红）/ 右 8（camp 1 蓝）；camp 0 在上面已经 return
            SetLayerAll(h.camp == 1 ? 8 : 7);
        }
    }
}
