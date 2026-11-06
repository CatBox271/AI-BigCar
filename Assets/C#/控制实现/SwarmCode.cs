using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SwarmCode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool Down;
    public bool Host = true;
    public string kind;

    DragUI dragUI;
    private void Start()
    {
        if (!Host)
        {
            Destroy(gameObject.GetComponent<DragUI>());
        }
        else
        {
            TryGetComponent(out dragUI);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Down = true;
        MX = Input.mousePosition.x;
    }
    float MX;
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Host) return;
        if (Down)
        {
            if (Fixed)
            {
                Fixed = false;
                GetCM();
                CM.SizeButton(IntPos.x + IntPos.y * 10);
            }
        }
        Down = false;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Down = false;
    }

    private void LateUpdate()
    {
        if (!Host)
        {
            if (!Fixed)
            {
                if (Input.GetMouseButton(0))
                {
                    transform.position = Input.mousePosition;
                    transform.localScale = Vector3.one;
                }
                else
                {
                    TryFixOn();
                    Fixed = true;
                    Down = false;
                }
            }
        }
        else
        {
            if (Down)
            {
                if (Mathf.Abs(Input.mousePosition.x - MX) > 100)
                {
                    Host = false;
                    Fixed = false;
                    Instantiate(gameObject).transform.SetParent(transform.parent.parent.GetChild(1));
                    Fixed = true;
                    dragUI.Stop();
                    Host = true;
                    Down = false;
                }
            }
        }
    }
    public bool Fixed = true;
    public Vector2Int IntPos = new(-1, -1);
    public SwarmCode Copy(int to, string add = "")
    {
        Host = false;
        Fixed = true;
        SwarmCode sc = Instantiate(gameObject).GetComponent<SwarmCode>();
        sc.transform.SetParent(transform.parent.parent.GetChild(1));
        if (add != "")
        {
            sc.kind += add;
            sc.transform.GetChild(0).GetComponent<InputField>().text = add;
        }
        sc.MoveTo(to);
        sc.SizePos();
        Host = true;
        return sc;
    }
    public void MoveTo(int toward)
    {
        IntPos = new(
            y: toward / 10,
            x: toward % 10
            );
        GetCM();
        CM.SizeButton(IntPos.x + IntPos.y * 10);
    }
    public void MoveTo(Vector2Int toward)
    {
        IntPos = toward;
        GetCM();
        CM.SizeButton(IntPos.x + IntPos.y * 10);
    }

    public void SizePos()
    {
        transform.localPosition = new Vector2(-202.5f, 202.5f) + new Vector2(1, -1) * IntPos * 45;
        transform.localScale = Vector3.one;
    }

    public void SizeUp()
    {
        SizePos();
        transform.SetAsLastSibling();
        GetCM();
        int Index = IntPos.x + IntPos.y * 10;
        if (CM.CodeList[Index] == this)//在Code中为最上层
        {
            float k = 1;
            CM.UpList[Index].ForEach(s =>
            {
                if (s != null)
                {
                    transform.localScale -= Vector3.one * 0.125f;
                    if (s.kind is "{")
                    {
                        transform.localPosition += 22.5f * (k - transform.localScale.x) * new Vector3(1, -1);
                    }
                    if (s.kind is "}")
                    {
                        transform.localPosition -= 22.5f * (k - transform.localScale.x) * new Vector3(1, -1);
                    }
                    k = transform.localScale.x;
                }
            });
        }
        else
        {
            for (int i = CM.UpList[Index].Count - 1; i > -1; i--)
            {
                if (CM.UpList[Index][i] == null) CM.UpList[Index].RemoveAt(i);
            }
            float k = 1;
            for (int i = 0; i < CM.UpList[Index].Count; i++)
            {
                SwarmCode sc = CM.UpList[Index][i];
                if (sc == this) return;
                transform.localScale -= Vector3.one * 0.125f;
                if (sc.kind is "{")
                {
                    transform.localPosition += 22.5f * (k - transform.localScale.x) * new Vector3(1, -1);
                }
                if (sc.kind is "}")
                {
                    transform.localPosition -= 22.5f * (k - transform.localScale.x) * new Vector3(1, -1);
                }
                k = transform.localScale.x;
            }
        }
    }
    CodeManager CM;
    CodeManager GetCM()
    {
        if(CM== null) CM = transform.parent.GetComponent<CodeManager>();
        return CM;
    }
    public void TryFixOn()
    {
        if (Mathf.Abs(transform.localPosition.x) > 225 || Mathf.Abs(transform.localPosition.y) > 225)
        {
            Destroy(gameObject);
            return;
        }
        Vector2Int last = IntPos;
        IntPos = new(
            x: Mathf.RoundToInt((transform.localPosition.x + 202.5f) / 45f),
            y: Mathf.RoundToInt((202.5f - transform.localPosition.y) / 45f)
            );
        transform.localPosition = new Vector2(-202.5f, 202.5f) + new Vector2(1, -1) * IntPos * 45;
        if (last != IntPos)
        {
            GetCM();
            if (new List<string>() { "{","}" }.Contains(kind)) CM.SetUp(IntPos.x + IntPos.y * 10, this);
            else CM.SetCode(IntPos.x + IntPos.y * 10, this);
        }
        GetCM();
        CM.SizeButton(IntPos.x + IntPos.y * 10);
    }
}
