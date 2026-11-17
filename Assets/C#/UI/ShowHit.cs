using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHit : MonoBehaviour
{
    public float LifeTime = 2f;//存活时间，到点回池

    static readonly Dictionary<GameObject, Stack<ShowHit>> Free = new();//空闲栈，取用 O(1)；原来是线性扫 List 找 activeSelf==false，池子涨到几千个时单次 600us
    static Transform root;
    static Transform Root//所有飘字挂在这个空物体下，不往场景根目录扔
    {
        get
        {
            if (root == null) root = new GameObject("飘字").transform;
            return root;
        }
    }

    GameObject source;
    private Txt t;
    private float baseSize;
    float hit;

    private void Awake()
    {
        TryGetComponent(out t);
        baseSize = t.size;
    }

    public static void Spawn(GameObject prefab, Vector3 pos, int camp, float num)
    {
        if (prefab == null) return;
        ShowHit item = null;
        if (Free.TryGetValue(prefab, out Stack<ShowHit> stack))
        {
            while (stack.Count > 0)
            {
                ShowHit candidate = stack.Pop();
                if (candidate == null) continue;
                item = candidate;
                break;
            }
        }
        if (item == null)
        {
            item = Instantiate(prefab).GetComponent<ShowHit>();
            if (item == null) return;
            item.source = prefab;
            item.transform.SetParent(Root, false);
        }
        item.transform.position = pos;
        item.transform.localScale = Vector3.one;
        item.gameObject.SetActive(true);
        item.Setting(camp, num);
        if (!item.inActive)
        {
            item.inActive = true;
            active.Add(item);
        }
    }

    static void Recycle(GameObject prefab, ShowHit item)
    {
        if (prefab == null || item == null) return;
        if (!Free.TryGetValue(prefab, out Stack<ShowHit> stack))
        {
            stack = new Stack<ShowHit>();
            Free[prefab] = stack;
        }
        item.inActive = false;
        stack.Push(item);
    }

    //原来每个飘字起一个协程，一次爆炸几百个飘字就是几百次 StartCoroutine(string)（按名字反射找方法）。
    //改成自己的 Update 推进：Unity 调 Update 是批处理的，比逐个开协程便宜，也不产生协程对象。
    static readonly Color FadeStep = new Color(0, 0, 0, 0.75f);
    static readonly List<ShowHit> active = new();//所有在飞的飘字，由 Clock 统一推进，各自不再挂 Update
    bool inActive;

    /// <summary>飘字推进间隔（秒）。0 = 每帧推进。调大减少调用次数，但位移是一跳一跳走的，别超过 0.033。</summary>
    public const float StepInterval = 0.015f;
    static readonly ClockBucket stepBucket = Clock.Bucket(StepInterval);

    static ShowHit()
    {
        stepBucket.OnDue += StepAll;//只在自己到期那一帧被叫
        fadeBucket.OnDue += FadeAll;
    }

    static void StepAll(float step)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ShowHit h = active[i];
            if (h == null)
            {
                active[i] = active[active.Count - 1];
                active.RemoveAt(active.Count - 1);
                continue;
            }
            if (h.Step(step)) continue;
            h.inActive = false;
            active[i] = active[active.Count - 1];
            active.RemoveAt(active.Count - 1);
        }
    }

    static void FadeAll(float passed)
    {
        for (int i = 0; i < active.Count; i++)
        {
            ShowHit h = active[i];
            if (h != null) h.Fade(passed);
        }
    }
    Vector3 startSpeed;
    float life;
    SpriteRenderer[] charSp = new SpriteRenderer[8];//字符的 SpriteRenderer 缓存，省掉每帧 GetChild+TryGetComponent
    int charCount = -1;
    /// <summary>颜色刷新间隔（秒）。越大写入越少；淡出总时长和曲线不受影响，步长按桶的 Passed 算。</summary>
    public const float FadeInterval = 0.1f;
    static readonly ClockBucket fadeBucket = Clock.Bucket(FadeInterval);//所有飘字共享同一个节拍，累加只算一次

    void RebuildCharCache()
    {
        charCount = transform.childCount;
        if (charSp.Length < charCount) charSp = new SpriteRenderer[Mathf.NextPowerOfTwo(charCount)];
        for (int i = 0; i < charCount; i++)
        {
            Transform c = transform.GetChild(i);
            charSp[i] = c.TryGetComponent(out SpriteRenderer sp) ? sp : null;
        }
    }

    /// <summary>按自己的淡出节拍擦一次颜色。</summary>
    void Fade(float passed)
    {
        if (charCount != transform.childCount) RebuildCharCache();
        Color step = passed * FadeStep;
        for (int i = 0; i < charCount; i++)
        {
            SpriteRenderer sp = charSp[i];
            if (sp != null) sp.color -= step;
        }
    }

    /// <summary>推进位置与计时，返回 false 表示结束、该回池了。</summary>
    bool Step(float dt)
    {
        if (life <= 0f)
        {
            gameObject.SetActive(false);//回池
            Recycle(source, this);
            return false;
        }
        if (charCount != transform.childCount) RebuildCharCache();

        //节拍由全局 Clock 统一算，这里只读一个 bool；步长用桶累计的 Passed，FadeInterval 随便调曲线都不变
        if (fadeBucket.Due)
        {
            Color step = fadeBucket.Passed * FadeStep;
            for (int i = 0; i < charCount; i++)
            {
                SpriteRenderer sp = charSp[i];
                if (sp != null) sp.color -= step;
            }
        }

        transform.position += startSpeed * dt;
        startSpeed -= new Vector3(0, 60f * dt, 0);
        life -= dt;
        return true;
    }

    public void Setting(int _camp, float _hit)
    {
        if (_camp == 1)
        {
            t.color = new Color(1f, 0.75f, 0.75f,0.75f);
        }
        if (_camp == 2)
        {
            t.color = new Color(0.75f, 0.75f, 1f,0.75f);
        }
        t.size = baseSize;//复用旧对象时字号被乘过，先复位
        // 间距、水平/竖直偏移都随 size 等比缩放，只需缩放字号
        t.size *= Mathf.Clamp(_hit / 30f, 1f, 3f);
        t.ApplyToChildren();//把复用到的那几个旧字符的颜色和字号刷回基准
        t.context = ((int)_hit).ToString();
        t.Flush();//Txt 平时挂 0.1 秒节拍刷，飘字刚生成这一下要立刻出字
        hit = _hit;
        //起飞初速和存活时间在这里初始化，交给 Update 推进
        transform.position += new Vector3(0, 1f, 0);
        float t01 = Mathf.Clamp(_hit / 200f, 1f, 5f);
        startSpeed = new Vector3(Random.Range(-3f, 3f), 3f * (Random.Range(0f, Mathf.Clamp(_hit / 200f, 2f, 5f)) + 7.5f * t01));
        life = LifeTime;
    }
}
