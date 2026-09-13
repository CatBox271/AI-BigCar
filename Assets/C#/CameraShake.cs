using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static float start = 0;
    private float num = 0;

    void Update()
    {
        float t = start - num;
        if (Mathf.Abs(t)<0.2f)
        {
            StartCoroutine("shake");
            num = start;
        }
        num = Time.time;
    }
    IEnumerator shake()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.position += new Vector3(0.01f, 0);
            yield return null;
        }
        for (int i = 0; i < 5; i++)
        {
            transform.position -= new Vector3(0.01f, 0);
            yield return null;
        }
    }
}
