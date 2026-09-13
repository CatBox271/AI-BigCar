using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWithCamera : MonoBehaviour
{
    Transform _camera;
    public float Times;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main.transform;
    }
    private void Update()
    {
        transform.position = new Vector3(_camera.transform.position.x * Times, transform.position.y);
    }
}
