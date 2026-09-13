using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarFollow : MonoBehaviour
{
    public Transform follow_with;
    public GameObject Star;
    public bool Inside;
    public void SetStar(Transform on, int grade)
    {
        follow_with = on;
        for (int i = 0; i < grade; i++)
        {
            GameObject go = Instantiate(Star, transform);
            go.transform.localPosition = new(i * 0.15f, 0, 0);
        }
        if (TryGetComponent(out TryConnect tc))
        {
            Inside = tc.Inside;
        }
    }

    private void LateUpdate()
    {
        if (GameManager.gameState == GameManager.GameState.Start)
        {
            transform.position = follow_with.position;
            transform.eulerAngles = new Vector3(0, 0, follow_with.eulerAngles.z);
            transform.parent = follow_with;
            Destroy(this);
        }
        else
        {
            if (follow_with != null)
            {
                transform.position = follow_with.position;
                transform.eulerAngles = new Vector3(0, 0, follow_with.eulerAngles.z);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
