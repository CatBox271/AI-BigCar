using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollShooter : MonoBehaviour
{
    public GameObject SelectBall;

    public float CoolTime;
    float RT = -1;//Red CoolTime
    float BT = -1;//Blue CoolTime
    public float AT = -1;
    private void FixedUpdate()
    {
        RT -= Time.fixedDeltaTime;
        BT -= Time.fixedDeltaTime;
        AT -= Time.fixedDeltaTime;
        if (RT < 0)
        {
            RT = CoolTime;
            ShootBall(true);
        }
        if (BT < 0)
        {
            BT = CoolTime;
            ShootBall(false);
        }
        if (AT < 0)
        {
            CoolTime /= 2;
            AT = 240f;
        }
    }

    void ShootBall(bool Red)
    {
        GameObject go= Instantiate(SelectBall, transform);
        SelectBall sb = go.GetComponent<SelectBall>();
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        sb.Red = Red;
        go.transform.position = transform.position;
        rb.velocity = Random.insideUnitCircle.normalized * 40f;
        go.transform.parent = transform.parent;
    }
}
