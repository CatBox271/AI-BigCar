using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedMove : MonoBehaviour
{
    public Transform follow_with;
    public Vector3 Offset;

    public void Set(Transform on,Vector3 _Offset )
    {
        follow_with = on;
        Offset = _Offset;
    }

    private void LateUpdate()
    {
        if (GameManager.gameState != GameManager.GameState.Start)
        {
            if (follow_with != null)
            {
                transform.position = follow_with.position + Offset;
                transform.eulerAngles = new Vector3(0, 0, follow_with.eulerAngles.z);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(this);
        }
    }
}
