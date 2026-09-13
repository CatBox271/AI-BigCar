using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedShow : MonoBehaviour
{
    public static Rigidbody2D center;
    public Text text;
    int a = 0;
    public AudioSource AS;
    private void FixedUpdate()
    {
        a++;
        if (a < 10) return;
        a = 0;
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            if (center == null)
            {
                if (Camera.main.GetComponent<LevelCamera>().all_cars.Count != 0)
                {
                    center = Camera.main.GetComponent<LevelCamera>().all_cars[0].GetComponent<Rigidbody2D>();
                }
                else
                {
                    AS.volume = 0;
                    return;
                }
            }
            float speed = center.velocity.magnitude;
            speed *= 3.6f;
            speed = Mathf.Round(speed * 10f) / 10f;
            string sp = speed.ToString();
            if (!sp.Contains("."))
            {
                sp += ".0";
            }
            text.text = sp + "km / h";
            if (speed > 60)
            {
                AS.volume = Mathf.Clamp(1 - (300 - speed) / 300f, 0, 1);
            }
            else
            {
                AS.volume = 0;
            }
        }
        else
        {
            text.text = "";
            center = null;
            AS.volume = 0;
        }
    }
}
