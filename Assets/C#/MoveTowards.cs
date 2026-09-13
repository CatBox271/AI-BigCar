using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    public Vector3 AimPos;
    private Vector3 r;
    private void Awake()
    {
        Destroy(gameObject, 2f);
    }
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, AimPos, ref r,1f);
    }
}
