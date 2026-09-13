using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SwarmBlock : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    public GameObject BlockKind;
    public int Num;
    public bool Down;
    public List<Heath> h;
    public Txt t;
    public static GameObject BlockChoose;
    private GameObject LastBlockChoose;
    public bool Infinity;
    private void Awake()
    {
        Refresh();
    }

    public void SetHeath()
    {
        if (BlockKind == null) return;
        GameObject load = Resources.Load("GameObject/" + BlockKind.name) as GameObject;
        GameObject go = Instantiate(load,transform);
        go.name = load.name;
        go.transform.localScale = load.transform.lossyScale;
        go.SetActive(false);
        BlockKind = go;
        Refresh();
    }
    public void Refresh()
    {
        if (BlockKind != null)
        {
            h = FindHeath(BlockKind.transform);
            string n = BlockKind.name;
            if (n.IndexOf("(") != -1)
            {
                n = n.Remove(n.IndexOf("("));
            }
            GetComponent<RawImage>().texture = Resources.Load("Sprite/" + n) as Texture;
            if (Num <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
    private void Update()
    {
        if (Infinity) Num = 10;
        //Follow Mouse
        if (mousePos != new Vector3(-1, -1, -1))
        {
            Vector3 Delat = Input.mousePosition - mousePos;
            transform.parent.GetComponent<OrderContainer>().RelativePos += new Vector2(Delat.x, 0) / transform.parent.localScale.x;
            mousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousePos = new(-1, -1, -1);
        }
        //
        if (Down && Time.time - DT > 0.25f)
        {
            if (!ReadyShow)
            {
                string n = BlockKind.name;
                if (n.IndexOf("(") != -1)
                {
                    n = n.Remove(n.IndexOf("("));
                }
                transform.parent.GetComponent<OrderContainer>().BI.SetName( n);
                ReadyShow = true;
            }
        }
        if (ReadyShow && !Down)
        {
            ReadyShow = false;
            transform.parent.GetComponent<OrderContainer>().BI.SetName( "");
        }
    }
    bool ReadyShow;

    int lastNum;
    void LateUpdate()
    {
        if (lastNum != Num)
        {
            lastNum = Num;
            t.context = Num.ToString();
        }
        if (LastBlockChoose != BlockChoose)
        {
            if (BlockChoose != null)
            {
                if (BlockChoose.CompareTag("Player"))
                {
                    if (transform.GetChild(0).gameObject.activeSelf)
                    {
                        Num--;
                        FollowMouse fm = SwarmObject();
                        fm.ClickTo(BlockChoose.transform);
                        BlockChoose = LastBlockChoose;

                        if (Num <= 0)
                        {
                            transform.GetChild(0).gameObject.SetActive(false);
                            BlockChoose = new();
                            gameObject.SetActive(false);
                        }
                        return;
                    }
                }
            }
            LastBlockChoose = BlockChoose;
            if (BlockChoose != null && BlockKind != null)
            {
                string A = BlockKind.name.IndexOf("(") == -1 ? BlockKind.name : BlockKind.name.Remove(BlockKind.name.IndexOf("("));
                string B = BlockChoose.name.IndexOf("(") == -1 ? BlockChoose.name : BlockChoose.name.Remove(BlockChoose.name.IndexOf("("));
                if (A == B)
                {
                    bool same = true;
                    if (same)
                    {
                        transform.GetChild(0).gameObject.SetActive(true);
                        return;
                    }
                }
            }
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    List<Heath> FindHeath(Transform father)
    {
        List<Heath> ah = new();
        if (father.TryGetComponent(out Heath h))
        {
            ah.Add(h);
        }
        for (int i = 0; i < father.childCount; i++)
        {
            ah.AddRange(FindHeath(father.GetChild(i)));
        }
        return ah;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Down = true;
        BlockChoose = BlockKind;
        mousePos = Input.mousePosition;
        DT = Time.time;
    }
    public float DT;
    Vector3 mousePos = new(-1, -1,-1);

    FollowMouse SwarmObject()
    {
        GameObject go = Instantiate(BlockKind);
        if (go.name.IndexOf("(") > 0)
        {
           go.name = go.name.Remove(go.name.IndexOf("("));
        }
        go.transform.localScale = BlockKind.transform.localScale;
        go.SetActive(true);
        return go.AddComponent<FollowMouse>();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Down)
        {
            Down = false;
            FollowMouse fm = SwarmObject();
            fm.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Num--;
        }
        if (Num <= 0 && ! Infinity)
        {
            gameObject.SetActive(false);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Down = false;
        mousePos = new(-1, -1, -1);
    }
}
