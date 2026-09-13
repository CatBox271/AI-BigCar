using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoom : MonoBehaviour
{
    private Heath heath;
    public GameObject BoomWave;

    public float Radius = 5.5f;
    public float Hit = 10f;

    private void Awake()
    {
        TryGetComponent(out heath);
    }

    bool Die;
    private void FixedUpdate()
    {
        if (Die) return;
        if (heath.KeepRecord[1] > 0 || heath.KeepRecord[2] > 0)
        {
            Boom();
        }
    }
    private void Boom()
    {
        Die = true;
        Color col = Color.white;
        int lay = 0;
        if (heath.camp == 1)
        {
            col = new Color(0.75f, 0.75f, 1f);
        }
        if (heath.camp == 2)
        {
            col = new Color(1f, 0.75f, 0.75f);
        }
        Instantiate(BoomWave, transform.position, transform.rotation).GetComponent<TNTWave>().Setting(col, lay, Radius, 30f, Hit, heath.camp);
        Destroy(gameObject);
    }
}
