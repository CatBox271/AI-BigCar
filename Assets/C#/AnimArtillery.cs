using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimArtillery : MonoBehaviour
{
    public bool ban;
    public bool stage_Right;
    private void OnDisable()
    {
        ban = true;
    }
    private void Update()
    {
        if (ban)
        {
            if (stage_Right)
            {
                StartCoroutine("animLeft");
            }
            else
            {
                StartCoroutine("animRight");
            }
            ban = false;
        }
    }
    IEnumerator animRight()
    {
        stage_Right = true;
        transform.localEulerAngles = new Vector3(0, 0, 180);
        do
        {
            transform.Rotate(Vector3.forward, -180 * Time.deltaTime);
            yield return null;
        }
        while (transform.localEulerAngles.z > 3);
        yield return new WaitForSeconds(1f);
        StartCoroutine("animLeft");
    }
    IEnumerator animLeft()
    {
        stage_Right = false;
        transform.localEulerAngles = new Vector3(0, 0, 0);
        do
        {
            transform.Rotate(Vector3.forward, 180 * Time.deltaTime);
            yield return null;
        }
        while (transform.localEulerAngles.z < 180);
        yield return new WaitForSeconds(1f);
        StartCoroutine("animRight");
    }
}
