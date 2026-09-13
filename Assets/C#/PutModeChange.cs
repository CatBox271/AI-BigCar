using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PutModeChange : MonoBehaviour
{
    public static bool Line;
    public Image image;
    bool Down;
    float t;
    Vector3 last;
    void Update()
    {
        if (GameManager.DeleteMode) return;
        if (GameManager.gameState != GameManager.GameState.Ready) return;
        if (Input.touchCount > 1) return;
        if (Input.GetMouseButton(0))
        {
            if (Down)
                t += Time.deltaTime / Time.timeScale;
            else
                t = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            t = 0;
            Down = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!BlockManager.bm.IsOnUIElement(Input.mousePosition))
            {
                Down = true;
                last = Input.mousePosition;
            }
        }
        if (t > 0.25f)
        {
            transform.position = Input.mousePosition;
            image.fillAmount = (t - 0.25f) / 0.25f;
            if ((t - 0.25f) / 0.5f >= 1f)
            {
                Line = true;
            }
        }
        else
        {
            image.fillAmount = 0;
            Line = false;
            if (Input.mousePosition != last)
            {
                if ((Input.mousePosition - last).magnitude > 10f)
                {
                    Down = false;
                }

            }
        }
    }
}
