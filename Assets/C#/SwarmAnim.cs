using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmAnim : MonoBehaviour
{
    //0 BadPig
    //1 BoxCat
    public string path;
    private void Awake()
    {
        if (Camera.main.name == "VideoMode")
        {
            path = "Anim/Center";
        }
        else
        {
            if (GameObject.Find("Canvas").TryGetComponent(out BlockManager bm))
            {
                path = bm.animPath;
            }
            else
            {
                path = "Anim/" + PlayerPrefs.GetString("animPath", "BadBig");
            }
        }
    }
    private void Start()
    {
        if (gameObject.transform.childCount == 0)
        {
            if (path != "")
            {
                Transform go = Instantiate(Resources.Load(path) as GameObject, transform).transform;
                go.localPosition += new Vector3(0, -0.431f, 0);
                go.localScale += new Vector3(0.65f, 0.65f, 0.65f);
            }
        }
    }
}
