using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCombine : MonoBehaviour
{
    public Transform aim;
    public bool Left = true;
    private void FixedUpdate()
    {
        if (Left)
        {
            if (aim.position.x < transform.position.x)
            {
                Combine();
            }
        }
        else
        {
            if (aim.position.x > transform.position.x)
            {
                Combine();
            }
        }
    }

    private void Combine()
    {
        VideoCameraFollow.state = (aim.parent.parent.position.x + transform.parent.parent.position.x) / 2f;
        Destroy(aim.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
