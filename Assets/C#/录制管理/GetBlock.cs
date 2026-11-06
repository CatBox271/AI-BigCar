using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBlock : MonoBehaviour
{
    public OwnCar Roc;
    public OwnCar Boc;
    public int BlockKind;
    public bool On;
    public RollShooter rs;
    public List<SpriteRenderer> spl;

    public static int red_num;
    public static int blue_num;


    public void SetOn(bool _on)
    {
        On = _on;
        if (_on)
        {
            for (int i = 0; i < spl.Count; i++)
            {
                Color c = spl[i].color;
                c.a = 1;
                spl[i].color = c;
            }
        }
        else
        {
            for (int i = 0; i < spl.Count; i++)
            {
                Color c = spl[i].color;
                c.a = 0.25f;
                spl[i].color = c;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!On) return;
        if (collision.collider.CompareTag("Balls"))
        {
            if (collision.collider.TryGetComponent(out SelectBall sb))
            {
                if (sb.Red)
                {
                    if (red_num > 10) return;
                    if (!Roc.Sending)
                    {
                        TryBLock(BlockKind, sb.Red);
                        StartCoroutine(nameof(SelectedLighted));
                        red_num++;
                    }
                }
                else
                {
                    if (blue_num > 10) return;
                    if (!Boc.Sending)
                    {
                        TryBLock(BlockKind, sb.Red);
                        StartCoroutine(nameof(SelectedLighted));
                        blue_num++;
                    }
                }
                Destroy(collision.gameObject);
            }
        }
    }
    List<Color> cl = new();
    private void OnValidate()
    {
        spl = new();
        spl.Add(GetComponent<SpriteRenderer>());
        for (int i = 0; i < transform.childCount; i++)
        {
            spl.Add(transform.GetChild(i).GetComponent<SpriteRenderer>());
        }
    }

    private void Awake()
    {
        red_num = 0;
        blue_num = 0;
        cl.Clear();
        for (int i = 0; i < spl.Count; i++)
        {
            cl.Add(spl[i].color);
        }
    }
    IEnumerator SelectedLighted()
    {
        for (int i = 0; i < spl.Count; i++)
        {
            spl[i].color = cl[i] * 1.5f;
        }
        for (int i = 0; i < 6; i++)
        {
            yield return null;
        }
        for (int i = 0; i < spl.Count; i++)
        {
            spl[i].color = cl[i];
        }
    }

    public void TryBLock(int kind,bool Red)
    {
        GameObject BlockKind = Resources.Load("GameObject/" + GameManager.BlockIndex[kind]) as GameObject;
        GameObject go = Instantiate(BlockKind);
        go.name = BlockKind.name;
        go.transform.localScale = BlockKind.transform.localScale;
        go.SetActive(true);
        if (Red)
        {
            go.transform.parent = Roc.transform.parent;
        }
        else
        {
            go.transform.parent = Boc.transform.parent;
        }

        Vector3 Offset = new(0,3);
        switch (kind)
        {
            case 2://BLOCK
                Offset = new Vector3(Random.Range(-4.5f, 4.5f), 3);
                break;
            case 51://drill
                Offset = new Vector3(3f, -1f);
                break;
            case 5://drill
                Offset = new Vector3(3f, -1f);
                break;
            case 1://Artillery
                if (Random.Range(0, 4) == 0)
                {
                    Offset = new Vector3(Random.Range(-4.5f, 4.5f), -3);
                }
                else
                {
                    Offset = new Vector3(Random.Range(-4.5f, 4.5f), 3);
                }
                break;
            case 40://waterWheel
                Offset = new Vector3(-2.25f, -2.25f);
                break;
            case 39://WaterPropellers
                Offset = new Vector3(-3f, -1f);
                break;
            case 26://Tail
                Offset = new Vector3(-3f, -1f);
                break;
        }
        if (Red)
        {
            go.transform.position = Roc.transform.position + Offset;
        }
        else
        {
            Offset.x *= -1;
            go.transform.position = Boc.transform.position + Offset;
        }
        go.AddComponent<FollowMouse>();
    }
}
