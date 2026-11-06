using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixingTrail : MonoBehaviour
{
    public FixingTool FT;
    TrailRenderer tr;
    void Start()
    {
        TryGetComponent(out tr);
        if (FT.AimH == null)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine("move", FT.AimH.transform.position);
        
        Destroy(gameObject,2f);
    }
    IEnumerator move(Vector3 aim)
    {
        
        
        
        
        ;
        transform.position = aim;
        do
        {
            tr.startColor -= new Color(0, 0, 0, Time.deltaTime * 1f);
            tr.endColor -= new Color(0, 0, 0, Time.deltaTime * 1f);
            yield return null;
        } while (true);
    }
}
