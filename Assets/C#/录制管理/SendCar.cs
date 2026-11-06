using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendCar : MonoBehaviour
{
    public OwnCar Roc;
    public OwnCar Boc;
    public RollShooter rs;
    public List<SpriteRenderer> spl;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Balls"))
        {
            if (collision.TryGetComponent(out SelectBall sb))
            {
                if (sb.Red)
                {
                    if (!Roc.Sending) Roc.SendCar = true;
                    GetBlock.red_num = 0;
                }
                else
                {
                    if (!Boc.Sending) Boc.SendCar = true;
                    GetBlock.blue_num = 0;
                }
            }
            Destroy(collision.gameObject);
        }
    }
}
