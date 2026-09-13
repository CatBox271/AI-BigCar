using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PropellerAnim : MonoBehaviour
{
    public float speed;
    private float XScale;
    private void Start()
    {
        XScale = transform.localScale.x;
    }
    private bool form;
    private void Update()
    {
        if (form)
        {
            transform.localScale = new Vector3(Mathf.MoveTowards(transform.localScale.x, XScale, speed * Time.deltaTime), transform.localScale.y, transform.localScale.z);
            if (transform.localScale.x == XScale)
            {
                form = false;
            }
        }
        else
        {
            transform.localScale = new Vector3(Mathf.MoveTowards(transform.localScale.x, -XScale, speed * Time.deltaTime), transform.localScale.y, transform.localScale.z);
            if (transform.localScale.x == -XScale)
            {
                form = true;
            }
        }
    }
}
