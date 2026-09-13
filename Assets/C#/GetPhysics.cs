using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPhysics : MonoBehaviour
{
    public float Times = 3.6f;
    public List<int> need;
    public Rigidbody2D rb;
    public float speed;
    public float speedA;
    public Vector2 vel;
    public Vector2 velA;
    public float ang;
    public float angA;

    float last_speed;
    Vector2 last_vel;
    float last_ang;
    private void FixedUpdate()
    {
        for (int i = 0; i < need.Count; i++)
        {
            switch (need[i])
            {
                case 0:
                    speed = rb.velocity.magnitude * Times;
                    break;
                case 1:
                    speedA = (rb.velocity.magnitude - last_speed) / Time.fixedDeltaTime * Times;
                    last_speed = rb.velocity.magnitude;
                    break;
                case 2:
                    vel = rb.velocity;
                    break;
                case 3:
                    velA = (rb.velocity - last_vel) / Time.fixedDeltaTime * Times;
                    last_vel = rb.velocity;
                    break;
                case 4:
                    ang = rb.angularVelocity;
                    break;
                case 5:
                    angA = (rb.angularVelocity - last_ang) / Time.fixedDeltaTime;
                    last_ang = rb.angularVelocity;
                    break;
            }
        }
    }
}
