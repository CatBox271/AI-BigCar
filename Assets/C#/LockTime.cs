using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockTime : MonoBehaviour
{
    public Text txt;
    public float T;

    private void Awake()
    {
        T += 1;
    }
    private void OnValidate()
    {
        txt.text = (int)T + "√Î";
    }

    private void Update()
    {
        if (T <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            txt.text = (int)T + "√Î";
        }
        T -= Time.deltaTime;
    }
}
