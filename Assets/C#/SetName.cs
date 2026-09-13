using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetName : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private void Awake()
    {
        origin = sc.kind;
    }
    string origin;
    public SwarmCode sc;
    public InputField InputField;
    public Text text;
    bool Down;
    public void RemoveEnter()
    {
        string end = InputField.text;
        end = end.Replace("\n", "");
        end = end.Replace(" ", "");
        InputField.text = end;
        //sc.TryFixOn();
        sc.Fixed = true;
        sc.Down = false;
    }
    public void SetValue(string value)
    {
        InputField.text = value;
    }
    public void EndEdit()
    {
        sc.kind = origin+InputField.text;
        text.raycastTarget = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Down = true;
        DT = Time.time + 0.3f;
    }
    float DT;
    public void OnPointerExit(PointerEventData eventData)
    {
        Down = false;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Down = false;
    }

    private void Update()
    {
        if (sc.Fixed)
        {
            if (Down)
            {
                if (DT < Time.time)
                {
                    text.raycastTarget = true;
                    InputField.Select();
                }
            }
        }
    }
}
