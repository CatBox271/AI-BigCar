using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCarDesign : MonoBehaviour
{
    public 车辆样板加载 red;
    public 车辆样板加载 blue;
    public int state;
    public static bool red_choosed;
    public static bool blue_choosed;


    public List<SpriteRenderer> spl;


    private void Awake()
    {
        red_choosed = false;
        blue_choosed = false;
        cl= new();
        for (int i = 0; i < spl.Count; i++)
        {
            cl.Add(spl[i].color);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Balls"))
        {
            if (collision.collider.TryGetComponent(out SelectBall sb))
            {
                if ((sb.Red && red_choosed) || (!sb.Red && blue_choosed)) return;
                if (sb.Red)
                {
                    red_choosed = true;
                    red.SetCarState(state);
                }
                else
                {
                    blue_choosed = true;
                    blue.SetCarState(state);
                }
                Destroy(collision.gameObject);
            }
        }
    }
    List<Color> cl;

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
