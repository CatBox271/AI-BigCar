using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drill_animation : MonoBehaviour
{
    void Update()
    {
        transform.localPosition -= new Vector3(2f * Time.deltaTime, 0f, 0f);
        if (transform.localPosition.x < -1f)
        {
            transform.localPosition = new Vector3(1f, 0f, 0f);
        }
    }
}
