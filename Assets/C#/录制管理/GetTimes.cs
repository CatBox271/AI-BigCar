using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTimes : MonoBehaviour
{
    public OwnCar Roc;
    public OwnCar Boc;
    public bool On;
    public int state;//0+1*2HP3Speed
    public int Num;
    public bool Red;
    public RollShooter rs;
    public List<SpriteRenderer> spl;
    List<Color> cl;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!On) return;
        if (collision.CompareTag("Balls"))
        {
            if (collision.TryGetComponent(out SelectBall sb))
            {
                if (sb.Red)
                {
                    if (!Roc.Sending)
                    {
                        switch (state)
                        {
                            case 0:
                                Roc.CarNum += Num;
                                break;
                            case 1:
                                Roc.CarNum *= Num;
                                break;
                        }
                    }
                    switch (state)
                    {
                        case 2://HP
                            Roc.HeathUP += Num;
                            break;
                        case 3://Speed
                            Roc.SpeedUP += Num;
                            break;
                        case 4://Hit
                            Roc.HitUP += Num;
                            break;
                        case 5://Hit
                            Roc.RangeUP += Num;
                            break;
                    }
                    StartCoroutine(nameof(SelectedLighted));
                }
                else
                {
                    if (!Boc.Sending)
                    {
                        switch (state)
                        {
                            case 0:
                                Boc.CarNum += Num;
                                break;
                            case 1:
                                Boc.CarNum *= Num;
                                break;
                        }
                    }
                    switch (state)
                    {
                        case 2://HP
                            Boc.HeathUP += Num;
                            break;
                        case 3://Speed
                            Boc.SpeedUP += Num;
                            break;
                        case 4://Hit
                            Boc.HitUP += Num;
                            break;
                        case 5://Hit
                            Boc.RangeUP += Num;
                            break;
                    }
                    StartCoroutine(nameof(SelectedLighted));
                }
                Destroy(collision.gameObject);
            }
        }
    }
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
    private void Awake()
    {
        cl = new();
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
}
