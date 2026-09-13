using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFollow : MonoBehaviour
{
    public  Text txt;
    private void Update()
    {
        GetComponent<Text>().text = txt.text;
    }
}
