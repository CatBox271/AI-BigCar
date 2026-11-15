using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHit : MonoBehaviour
{
    public float LifeTime = 2f;//存活时间，到点回池

    static readonly Dictionary<GameObject, List<ShowHit>> Pools = new();
    static Transform root;
    static Transform Root//所有飘字挂在这个空物体下，不往场景根目录扔
    {
        get
        {
            if (root == null) root = new GameObject("飘字").transform;
            return root;
        }
    }

    List<ShowHit> pool;
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
        if (!Pools.TryGetValue(prefab, out List<ShowHit> list))
        {
            list = new List<ShowHit>();
            Pools[prefab] = list;
        }
        ShowHit item = null;
        for (int i = list.Count - 1; i > -1; i--)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
                continue;
            }
            if (list[i].gameObject.activeSelf) continue;
            item = list[i];
            break;
        }
        if (item == null)
        {
            item = Instantiate(prefab).GetComponent<ShowHit>();
            item.pool = list;
            item.transform.SetParent(Root, false);
            list.Add(item);
        }
        item.transform.position = pos;
        item.transform.localScale = Vector3.one;
        item.gameObject.SetActive(true);
        item.Setting(camp, num);
    }

    IEnumerator anim()
    {
        transform.position += new Vector3(0, 1f, 0);
        Vector3 startSpeed = new(Random.Range(-3f, 3f), 3f*(Random.Range(0f, Mathf.Clamp(hit / 200f,2,5)) +  7.5f * Mathf.Clamp(hit/ 200f, 1f, 5f)));
        float life = LifeTime;
        do
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out SpriteRenderer sp))
                {
                    sp.color -= Time.deltaTime * new Color(0,0,0,0.75f);
                }
            }
            transform.position += startSpeed * Time.deltaTime;
            startSpeed -= new Vector3(0, 60f * Time.deltaTime);
            life -= Time.deltaTime;
            yield return null;

        } while (life > 0);
        gameObject.SetActive(false);//回池，SetActive(false) 会停掉这个协程
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
        hit = _hit;
        StopCoroutine(nameof(anim));
        StartCoroutine(nameof(anim));
    }
}
