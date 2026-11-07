using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardCostBar : MonoBehaviour
{
    public int MaxCost;
    public int Cost;
    public Image prebar;
    public Image bar;

    private void OnValidate()
    {
        prebar.fillAmount = bar.fillAmount = (float)Cost / Mathf.Max(MaxCost, 1);
    }
}
