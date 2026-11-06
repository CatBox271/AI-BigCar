using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCrush : MonoBehaviour
{
    public List<GameObject> allEnter = new();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CompareTag("GameController")) return;
        allEnter.Add(collision.gameObject);
    }
    public int FrameSolve;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(nameof(Late));
    }

    List<List<Transform>> saveCalculate = new() {null,null,null,null};
    // 0 Engine 1Gear 2Stealth 3 Grosope
    public void Back(int k, List<Transform> all)
    {
        saveCalculate[k].AddRange(all);
    }
    IEnumerator Late()
    {
        do
        {
            for (int i = 0; i < FrameSolve; i++)
            {
                if (allEnter.Count > 0)
                {
                    GameObject go = allEnter[0];
                    if (go != null)
                    {
                        int r = go.name.IndexOf("(");
                        string Name = go.name;
                        if (r != -1)
                        {
                            Name = go.name.Remove(r, Name.Length - r);
                        }
                        bool Pass = false;
                        switch (Name)
                        {
                            case "BroadswordMines":
                                if (go.TryGetComponent(out BroadswordMines bm))
                                {
                                    bm.QD();
                                }
                                break;
                        }
                        if (Pass)
                        {
                            allEnter.Add(allEnter[0]);
                        }
                    }
                    allEnter.RemoveAt(0);
                }
            }
            yield return null;
        } while (true);
    }
}
