using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Txt : MonoBehaviour
{

    public GameObject Word;
    public string context;
    string last;
    public List<string> con;
    private int length;
    [Tooltip("字号。字间距、水平偏移、竖直偏移都按它等比缩放")]
    public float size = 15f;
    [Tooltip("水平偏移 = offset × size")]
    public float offset;
    [Tooltip("竖直偏移 = VerticalOffset × size")]
    public float VerticalOffset;
    public Color color = Color.white;
    [Tooltip("字间距 = word_offset × size")]
    public float word_offset = 0.05f;

    /// <summary>字间距，随 size 缩放。</summary>
    public float Spacing => word_offset * size;
    /// <summary>水平偏移，随 size 缩放。</summary>
    public float OffsetX => offset * size;
    /// <summary>竖直偏移，随 size 缩放。</summary>
    public float OffsetY => VerticalOffset * size;

    void OnValidate()
    {
        ApplyToChildren();
    }

    /// <summary>把当前的 size / offset / VerticalOffset / word_offset 立即应用到已经生成的字上。</summary>
    public void ApplyToChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.TryGetComponent(out ShowNum sn))
            {
                sn.Set(sn.num, size, OffsetX, color, Spacing, OffsetY);
                sn.Refresh();
            }
            else if (child.TryGetComponent(out CanvasShowNum csn))
            {
                csn.Set(csn.num, size, OffsetX, color, Spacing, OffsetY);
                csn.Refresh();
            }
        }
    }

    void Update()
    {
        if (context.Length > length)
        {
            for (int i = length; i < context.Length; i++)
            {
                GameObject swarm = Instantiate(Word, transform);
                if (swarm.TryGetComponent(out ShowNum sn))
                {
                    sn.Set(i, size, OffsetX, color, Spacing, OffsetY);
                }
                if (swarm.TryGetComponent(out CanvasShowNum sn1))
                {
                    sn1.Set(i, size, OffsetX, color, Spacing, OffsetY);
                }
                swarm.gameObject.layer = gameObject.layer;
            }
        }
        length = context.Length;
        if (last != context)
        {
            last = context;
            con.Clear();
            for(int i = 0;i<context.Length;i++)
            {
                con.Add(context[i].ToString());
            }
        }
    }
}
