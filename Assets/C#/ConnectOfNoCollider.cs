using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectOfNoCollider : MonoBehaviour
{
    public TryConnect tc;
    int a = 0;

    private void Awake()
    {
        a = Random.Range(0, 11);
    }

    private void FixedUpdate()
    {
        a++;
        if (a < 10) return;
        a = 0;
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            Destroy(this);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(i).gameObject.SetActive(tc.CantConnect[i]);
        }
    }
}
