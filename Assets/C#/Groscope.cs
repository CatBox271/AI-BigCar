using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Groscope : MonoBehaviour
{
    private Rigidbody2D rrb;
    bool Work;
    private void OnDestroy()
    {
        SetValue(false);
    }
   

    public List<Transform> AllBlock = new();

    int first = 4;

    private void Awake()
    {
        first += Random.Range(0, 3);
    }
    public void Search()
    {
        AllBlock = new();
        GetComponent<CheckAlive>().Search(new List<string>() { "" }, true, new()).ForEach(s => AllBlock.Add(s.transform));
        SetValue(true);
    }
    private void SetValue(bool value)
    {
        //更改目标值
        for (int i = 0; i < AllBlock.Count; i++)
        {
            Transform a = AllBlock[i];
            if (a != null)
            {
                if (a.TryGetComponent(out PowerWheel _)) continue;
                if (a.TryGetComponent(out Rigidbody2D grb))
                {
                    grb.freezeRotation = value;
                }
            }
        }
    }
    public bool On = true;
    private void FixedUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            first--;
            if (first == 0)
            {
                Search();

            }
            if (first < 0)
            {
                SetValue(On);
            }
        }

    }
    
}
