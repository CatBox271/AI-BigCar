using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Final : MonoBehaviour
{
    public GameObject catchit;
    float time;
    private void Update()
    {
        if (GameManager.gameState == GameManager.GameState.Ready)
        {
            time = Time.time;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - time < 0.5f) return;
        if (GameManager.gameState != GameManager.GameState.TurnWin && GameManager.gameState != GameManager.GameState.Win)
        {
            if (collision.TryGetComponent(out Heath h))
            {
                if (h.HP > 0)
                {
                    if (h.camp == 1)
                    {
                        GameManager.gameState = GameManager.GameState.TurnWin;
                        catchit = collision.gameObject;
                        transform.parent.GetComponent<Animator>().Play("FlagWin");
                    }
                }
            }
        }
    }
}
