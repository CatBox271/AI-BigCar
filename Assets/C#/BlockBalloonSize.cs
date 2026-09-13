using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBalloonSize : MonoBehaviour
{
    public Balloon ball;
    public Transform show_value;
    private void Update()
    {
        show_value.transform.localPosition = new Vector3(0, (4 - ball.value) / 8f);
        show_value.transform.localScale = new Vector3(1, ball.value / 4f, 1);
    }
}
