using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Txt : MonoBehaviour
{

    public GameObject Word;
    public string context;
    string last;
    public List<string> con;

    //字符字符串查表：飘字数字只有数字/小数点/负号，原来每个字符都 char.ToString() 分配一个 string
    static readonly string[] charCache = BuildCharCache();
    static string[] BuildCharCache()
    {
        string[] arr = new string[128];
        for (int i = 0; i < arr.Length; i++) arr[i] = ((char)i).ToString();
        return arr;
    }
    static string CharStr(char c)
    {
        return c < 128 ? charCache[c] : c.ToString();
    }
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

    //原来每个 Txt 每帧都在查 context 变没变、字符够不够。改成挂到 Clock 的 0.1 秒桶上统一刷，
    //外部改完 context 想立刻生效就自己调一次 Flush()（ShowHit.Setting 里就是这么做的）。
    public const float RefreshInterval = 0.1f;
    static readonly ClockBucket refreshBucket = Clock.Bucket(RefreshInterval);
    static readonly List<Txt> all = new();
    bool listed;

    static Txt()
    {
        refreshBucket.OnDue += StepAll;//0.1 秒到期才刷一遍，不是每帧
    }

    static void StepAll(float passed)
    {
        for (int i = all.Count - 1; i >= 0; i--)
        {
            Txt t = all[i];
            if (t == null)
            {
                all[i] = all[all.Count - 1];
                all.RemoveAt(all.Count - 1);
                continue;
            }
            t.Flush();
        }
    }

    void OnEnable()
    {
        if (listed) return;
        listed = true;
        all.Add(this);
    }

    void OnDisable()
    {
        if (!listed) return;
        listed = false;
        all.Remove(this);
    }

    /// <summary>按当前 context 补齐字符并重建字符表。改完 context 想立刻生效就调它。</summary>
    public void Flush()
    {
        //按实际子物体数量补字符：context 变短时 ShowNum 会自毁，拿字段记数量会和子物体对不上
        for (int i = transform.childCount; i < context.Length; i++)
        {
            GameObject swarm = Instantiate(Word, transform);
            if (swarm.TryGetComponent(out ShowNum sn))
            {
                sn.Set(i, size, OffsetX, color, Spacing, OffsetY);
                sn.Refresh();
            }
            if (swarm.TryGetComponent(out CanvasShowNum sn1))
            {
                sn1.Set(i, size, OffsetX, color, Spacing, OffsetY);
                sn1.Refresh();
            }
            swarm.gameObject.layer = gameObject.layer;
        }
        if (last != context)
        {
            last = context;
            con.Clear();
            for(int i = 0;i<context.Length;i++)
            {
                con.Add(CharStr(context[i]));
            }
        }
    }
}
