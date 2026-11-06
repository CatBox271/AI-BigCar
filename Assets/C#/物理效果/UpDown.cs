using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDown : MonoBehaviour
{
    public Vector3 speed;
    public float _time;
    float StartTime;
    private void OnEnable()
    {
        StartTime = Time.time;
    }
    void Update()
    {
        transform.position += Mathf.Cos((Time.time - StartTime) * _time) * speed * Time.deltaTime;
    }
}
