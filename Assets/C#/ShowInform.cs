using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowInform : MonoBehaviour
{
    public int Inform;
    public RougeChoose rc;
    public Text txt;
    void Update()
    {
        txt.text = rc.allScene[0][Inform];
    }
}
