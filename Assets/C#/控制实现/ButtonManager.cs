using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject Button;
    public  List<Transform> all_button = new() { };
    bool show;
    public void ClearButton()//Çå¿Õbutton
    {
        all_button.Clear();
        for (int i = transform.childCount - 1; i > -1; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public void AddButton(GameManager.ButtonInfo BI,CheckAlive ca)//Ìí¼Óbutton
    {
        GameObject go = Instantiate(Button, transform);
        all_button.Add(go.transform);
        go.GetComponent<ButtonState>().Set(BI._name, BI._forward,BI._state, BI._cd, BI._limit, BI._set_on, BI._set_num,ca,BI.IC != null);
        Order();
    }
    private void Update()
    {
        if (show)
        {
            if (GameManager.gameState == GameManager.GameState.Ready)
            {
                show = false;
                ClearButton();
            }
        }
        else
        {
            if (GameManager.gameState == GameManager.GameState.Start)
            {
                show = true;
            }
        }
    }

    public void Order()
    {
        float All = 1200f;
        float Blockwidth = 75f;

        float width = All;

        float center = width * 0.5f - All / 2f;
        int max = (int)((width) / Blockwidth);
        if (all_button.Count != 0)
        {
            for (int ii = 0; ii < all_button.Count; ii++)
            {
                if (all_button[ii] == null)
                {
                    all_button.RemoveAt(ii);
                    ii--;
                }
            }
            for (int ii = 0; ii < all_button.Count; ii++)
            {
                float X, Y;

                if (all_button.Count > max)
                {
                    int ni = ii % max;
                    X = center + (ni + 0.5f) * Blockwidth - width / 2f;
                    Y = ((ii - ni) / max) * Blockwidth - Blockwidth;
                }
                else
                {
                    X = center + (all_button.Count / -2f + ii + 0.5f) * Blockwidth;
                    Y = -Blockwidth;
                }
                all_button[ii].localPosition = new Vector2(X, Y);
            }
        }
    }
}
