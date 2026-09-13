using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaveName : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public string origin;
    public Button button;
    public InputField InputField;
    public Text text;
    public SaveMode sm;
    bool Down;
    public void RemoveEnter()
    {
        string end = InputField.text;
        end = end.Replace("\n", "");
        end = end.Replace(" ", "");
        InputField.text = end;
    }
    public void SetValue(string value)
    {
        InputField.text = value;
    }
    public void EndEdit()
    {
        text.raycastTarget = false;
        if (origin != InputField.text)
        {
            sm.Rename(origin,InputField.text);
            origin = InputField.text;
        }
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
