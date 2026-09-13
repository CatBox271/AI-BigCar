using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Vertical;//´¹Ö±
    public UIMoveBar UI_MoveBar;
    Vector3 mousePos = new(-1, -1, -1);

    public void Stop()
    {
        mousePos = new(-1, -1, -1);
    }
    private void Update()
    {
        //Follow Mouse
        if (mousePos != new Vector3(-1, -1, -1))
        {
            Vector3 Delat = Input.mousePosition - mousePos;
            if (Vertical)
            {
                UI_MoveBar.RelativePos += new Vector2(0, Delat.y) / UI_MoveBar.transform.lossyScale.y;
            }
            else
            {
                UI_MoveBar.RelativePos += new Vector2(Delat.x, 0) / UI_MoveBar.transform.lossyScale.x;
            }
            mousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousePos = new(-1, -1, -1);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        mousePos = Input.mousePosition;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        mousePos = new(-1, -1, -1);
    }
}
