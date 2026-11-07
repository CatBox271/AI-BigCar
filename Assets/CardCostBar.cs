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
    public Txt costNum;

    public float DownWaitTime;

    private float allH;
    private float minH;
    private float startH;

    private int lastCost;
    private void Refresh(bool immdiate = false)
    {
        if (lastCost == Cost) return;
        if (immdiate)
        {
            prebar.fillAmount = bar.fillAmount = (float)Cost / Mathf.Max(MaxCost, 1);
            StopCoroutine(nameof(BarFill));
        }
        else
        {
            prebar.fillAmount = (float)Cost / Mathf.Max(MaxCost, 1);
            if (lastCost > Cost)
            {
                StartCoroutine(BarFill(DownWaitTime, prebar.fillAmount));
            }
            else
            {
                StopCoroutine(nameof(BarFill));
                bar.fillAmount = prebar.fillAmount;
            }
        }
        costNum.transform.localPosition = new Vector3(0, startH + minH * Mathf.Clamp(Cost - 1, 0, MaxCost - 1));
        costNum.context = Cost.ToString();

        lastCost = Cost;
    }
    private void Update()
    {
        Refresh(false);
    }
    private void Start()
    {
        allH = prebar.rectTransform.rect.height;
        minH = allH / MaxCost;
        startH = (minH - allH) / 2f;
    }

    IEnumerator BarFill(float down,float fill)
    {
        yield return new WaitForSeconds(down);
        bar.fillAmount = fill;
    }
}
