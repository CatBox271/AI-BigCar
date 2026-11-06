using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeathToArtillery : MonoBehaviour
{
    public Heath h;
    public Artillery a;
    void Update()
    {
        a.camp = h.camp;
    }
}
