using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPShow : MonoBehaviour
{
    private Heath h;
    private Txt t;

    private void Awake()
    {
        TryGetComponent(out h);
        TryGetComponent(out t);
    }

    private void Update()
    {
        t.context = ((int)h.HP).ToString();
    }
}
