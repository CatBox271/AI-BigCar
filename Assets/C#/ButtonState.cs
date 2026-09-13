using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ButtonState : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public RectTransform rect;
    public string ButtonKind;
    public int forward = 0;//0U1D2L3R
    public int StateButton;
    public bool state;
    public float Kstate;
    public float CoolTime;
    public Vector3Int limit;

    public Image image;
    public RawImage RI;
    public RawImage ADD;
    public Text ButtonCalled;
    public Text text;

    public Button button;
    public Image CD;

    bool CoolDuration = true;

    CheckAlive center;
    public void Set(string _name,int _forward,int _state,float _cd,Vector3Int _limit,bool SB,float SN,CheckAlive ca, bool IsIC)
    {
        ButtonKind = _name;
        forward = _forward;
        StateButton = _state;
        WhenTouch += StateButton switch
        {
            0 => A0,
            1 => A1,
        };
        CoolTime = _cd;
        limit = _limit;
        state = SB;
        Kstate = SN;
        center = ca;
        switch (forward)
        {
            case 0:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case 1:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case 2:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case 3:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -90);
                break;
        }
        CD.enabled = CoolTime != 0;
        switch (StateButton)
        {
            case 1:
                image.color = Color.white;
                text.gameObject.SetActive(true);
                text.text = Keep2(Kstate);
                ADD.enabled = true;
                break;
        }
        if (IsIC)
        {
            if (StateButton == 0)
            {
                image.color = Color.cyan;
                text.transform.localPosition = new(0, -8.178f);
                ButtonCalled.gameObject.SetActive(true);
                ButtonCalled.text = _name;
                RI.enabled = false;
            }
            if (StateButton == 1)
            {
                ButtonCalled.gameObject.SetActive(true);
                ButtonCalled.text = _name;
                RI.enabled = false;
            }
        }
        name = _name;
        transform.GetChild(0).GetComponent<RawImage>().texture = Resources.Load("Sprite/" + _name) as Texture;

        
    }
    Action WhenTouch;

    void A0()
    {
        state = !state;
        GameManager.ButtonSet(center.VID, ButtonKind, forward, state, float.NaN);
    }
    void A1()
    {
        if (LKstate != Kstate) return;
        if (Input.mousePosition.x > rect.position.x)
        {
            Kstate++;
        }
        else
        {
            Kstate--;
        }
        Kstate = Limit(Kstate);
        text.text = Keep2(Kstate);
        GameManager.ButtonSet(center.VID, ButtonKind, forward, false, Kstate);
    }

    string Keep2(float k)
    {
        return k.ToString("0.#");
    }
    public void Get()//button接口
    {
        WhenTouch();
    }
    IEnumerator Cool(int time)
    {
        button.interactable = false;
        CD.fillAmount = 1;
        CoolTime = time;
        do
        {
            yield return null;
            CoolTime -= Time.deltaTime;
            CD.fillAmount = CoolTime / time;
        } while (CoolTime > 0);
        CD.fillAmount = 0;
        button.interactable = true;
    }

    IEnumerator FlashActive()
    {
        state = true;
        yield return new WaitForSeconds(0.05f);
        state = false;
    }

    private void Update()
    {
        switch (StateButton)
        {
            case 0:
                if (state)
                {
                    image.color = Color.green;
                    transform.GetChild(0).localPosition = new Vector3(Mathf.Sin(Time.time * 50f), Mathf.Cos(Time.time * 50f));
                }
                else
                {
                    image.color = Color.red;
                }
                break;
            case 1:
                if (Roll)
                {
                    Kstate = Limit(KValue());
                    text.text = Keep2(Kstate);
                    GameManager.ButtonSet(center.VID, ButtonKind, forward, false, Kstate);
                }
                break;
        }
    }
    float Limit(float k)
    {
        if (Mathf.Abs(limit.z) >= 2)//有限制
        {
            if (Mathf.Abs(limit.z) == 3)//有限制切循环
            {
                bool A = limit.x <= k && k <= limit.y;
                do
                {
                    if (k > limit.y) k -= limit.y - limit.x;
                    if (k < limit.x) k += limit.y - limit.x;
                    A = limit.x <= k && k <= limit.y;
                }
                while (!A);
            }
            k = Mathf.Clamp(k, limit.x, limit.y);
        }
        if (limit.z < 0) k = (int)k;
        return k;
    }
    float kd = -1;
    float KValue()
    {
        if (kd == -1)
        {
            if (Mathf.Abs(limit.z) >= 2) kd = Mathf.Clamp((limit.y - limit.x) / 20f, 0.25f, 5f);
            else kd = 2f;
        }
        float v3 = Input.mousePosition.x - LM;
        return LKstate + v3 * LSV / transform.lossyScale.x * kd;
    }

    public float LSV = 1;
    bool Roll;
    float LM = new();
    float LKstate = new();
    public void OnPointerDown(PointerEventData eventData)
    {
        LM = Input.mousePosition.x;
        LKstate = Kstate;
        Roll = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Roll = false;
    }
}
