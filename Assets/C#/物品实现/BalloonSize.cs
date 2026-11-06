using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSize : MonoBehaviour
{
    public SpriteRenderer sp;
    public Balloon ball;
    private void FixedUpdate()
    {
        sp.size = Vector2.one * (ball.value * 0.75f + 0.25f);
    }
}
