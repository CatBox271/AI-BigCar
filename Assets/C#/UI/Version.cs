using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour
{
    public Text txt;
    void Start()
    {
        txt.text = "版本号："+ Application.version;
    }

}
