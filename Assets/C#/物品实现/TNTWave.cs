using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNTWave : MonoBehaviour
{
    public int camp;
    private SpriteRenderer sp;
    private float radio;
    private float speed;
    private Color start_color;
    private float hit;
    private GameObject source;//从哪个 prefab 来的，回池要按来源分组
    private Transform tf;   //缓存，省掉每次回调都走一遍 native 的 transform 取值
    private float scaleX = 0.2f;
    private float curValue;//这一帧的伤害/推力系数，Update 里算一次，回调直接用
    private readonly HashSet<int> hitOnce = new();//同一个波形对同一个碰撞体只结算一次，物理抖动重复 enter 直接丢掉

    //同 prefab 的波形池：每次爆炸都 Instantiate + Destroy 太贵，复用同一批对象
    static readonly Dictionary<GameObject, Stack<TNTWave>> pools = new();
    static Transform poolRoot;
    static Transform PoolRoot
    {
        get
        {
            if (poolRoot == null)
            {
                GameObject go = GameObject.Find("[TNTWave]");
                if (go == null) go = new GameObject("[TNTWave]");
                poolRoot = go.transform;
            }
            return poolRoot;
        }
    }

    public static TNTWave Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;
        if (pools.TryGetValue(prefab, out Stack<TNTWave> stack))
        {
            while (stack.Count > 0)
            {
                TNTWave w = stack.Pop();
                if (w == null) continue;
                w.transform.SetParent(PoolRoot, false);//一律住在 [TNTWave] 里，不往场景根上摆
                w.transform.position = pos;
                w.transform.rotation = rot;
                w.gameObject.SetActive(true);
                return w;
            }
        }
        TNTWave nw = Instantiate(prefab, pos, rot, PoolRoot).GetComponent<TNTWave>();
        if (nw != null) nw.source = prefab;
        return nw;
    }

    void Recycle()
    {
        if (source == null)
        {
            Destroy(gameObject);
            return;
        }
        gameObject.SetActive(false);
        transform.SetParent(PoolRoot, false);
        if (!pools.TryGetValue(source, out Stack<TNTWave> stack))
        {
            stack = new Stack<TNTWave>();
            pools[source] = stack;
        }
        stack.Push(this);
    }

    public void Setting(Color _color, int _layer, float _radio, float _speed,float _hit,int _camp)
    {
        if (tf == null) tf = transform;
        TryGetComponent(out sp);
        sp.color = _color;
        scaleX = 0.2f;
        hitOnce.Clear();
        tf.localScale = Vector3.one * scaleX;//从一个小点炸开，别一上来就和方块一样大
        gameObject.layer = _layer;
        radio = _radio;
        speed = _speed;
        start_color = _color;
        hit = _hit;
        curValue = hit * (radio / scaleX);//首帧就有一个有效值，别让第一下碰撞读到 0
        camp = _camp;
        CameraShake.start = Time.time;
    }
    void Update()
    {
        if (scaleX > radio)
        {
            Recycle();
            return;
        }
        scaleX += speed * Time.deltaTime;
        tf.localScale = Vector3.one * scaleX;
        Color gh = (radio - scaleX) / radio * start_color * 2f;
        gh.a = Mathf.Min(0.9f, gh.a);
        sp.color = gh;
        curValue = hit * (radio / scaleX);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hitOnce.Add(collision.GetInstanceID())) return;//这个波形已经打过它了
        if (tf == null) tf = transform;
        Vector2 self = tf.position;//读一次 native 位置，下面两个分支共用
        float value = curValue;//Update 里算好的，回调用
        if (collision.TryGetComponent(out Heath h))
        {
            //h.enabled = true;  全工程没有地方把 Heath 关掉，这句每次都是空转
            if (h.camp == camp) h.Hit(value / 20f, camp);
            else h.Hit(value, camp);
        }
        Rigidbody2D rb = collision.attachedRigidbody;//直接拿碰撞体挂着的刚体，不用再查一次组件
        if (rb != null)
        {
            Vector2 n = rb.position - self;
            float len = n.magnitude;
            if (len < 0.0001f) return;//正好重合时没有方向，跳过
            float m = rb.mass < 1f ? 1f : rb.mass;
            rb.velocity += n * (value / len) / (m * m) / 5f;//原来是 Mathf.Pow(mass,2)，直接乘
        }
    }
}
