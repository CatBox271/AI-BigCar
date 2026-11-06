using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightLong : MonoBehaviour
{
    bool AL;
    void FixedUpdate()
    {
        if (transform.position.y > -4.5f)
        {
            if (transform.position.y > 0f)
            {
                if (!AL)
                {
                    AL = true;
                    if (TryGetComponent(out Artillery a))
                    {
                        a.Radius *= 2f;
                    }
                }
            }
            else
            {
                if (AL)
                {
                    AL = false;
                    if (TryGetComponent(out Artillery a))
                    {
                        a.Radius /= 2f;
                    }
                }
            }
        }
        else
        {
            if (transform.position.y > -44.99f)
            {
                if (!AL)
                {
                    AL = true;
                    if (TryGetComponent(out Artillery a))
                    {
                        a.Radius *= 2f;
                    }
                }
            }
            else
            {
                if (AL)
                {
                    AL = false;
                    if (TryGetComponent(out Artillery a))
                    {
                        a.Radius /= 2f;
                    }
                }
            }
        }
    }
}
