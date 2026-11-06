using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedFollow : MonoBehaviour
{
    public Transform follow;

    IEnumerator Before()
    {
        yield return new WaitForFixedUpdate();
        transform.position = follow.position;
    }
    private void FixedUpdate()
    {
        StartCoroutine(nameof(Before));
        //transform.position = follow.position;
    }
}
