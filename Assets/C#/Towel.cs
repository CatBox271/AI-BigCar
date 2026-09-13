using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Towel : MonoBehaviour
{
    public int camp;
    public float HP;
    private Color _color_start;
    private float max;
    private SpriteRenderer sp;
    public bool die;
    // Start is called before the first frame update
    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        max = HP;
        switch (camp)
        {
            case 1:
                sp.color = Color.red;
                break;
            case 2:
                sp.color = Color.blue;
                break;
        }
        _color_start = sp.color;
    }
    // Update is called once per frame
    void Update()
    {
        if (HP > 0)
        {
            Color a = _color_start;
            if (max != 0)
            {
                a *= HP / max;
                a.a = 1f;
            }
            else
            {
                a = Color.black;
            }
            sp.color = a;
        }
        else
        {
            if (die)
            {
                sp.color -= new Color(0, 0, 0, 0.5f) * Time.deltaTime;
                transform.localScale += Vector3.one * 1f * Time.deltaTime;
            }
            else
            {
                Destroy(gameObject, 1f);
                sp.color = _color_start / 2f;
                die = true;
            }
        }
    }
}
