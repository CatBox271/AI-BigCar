using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailOut : MonoBehaviour
{
    private TrailRenderer tr;
    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent(out tr);
    }
    void Update()
    {
        tr.startColor -= Color.white * Time.deltaTime *0.1f;
        tr.endColor -= Color.white * Time.deltaTime * 0.1f;
    }
}
