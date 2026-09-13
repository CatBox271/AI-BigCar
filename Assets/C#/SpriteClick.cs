using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteClick : MonoBehaviour
{
    public Animator anim;
    float lt;
    public int TC;
    public string nn;
    public Collider2D _collider;
    bool finish = true;

    public void Die()
    {
        anim.Play("die");
        Destroy(this);
    }

    private void Update()
    {
        if (!finish)
        {
            if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == nn)
            {
                for (int i = 1; i < anim.layerCount; i++)
                {
                    anim.SetLayerWeight(i, 1);
                }
                finish = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                lt = Time.time;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                if (Time.time - lt < 0.2f)
                {
                    if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == nn)
                    {
                        for (int i = 1; i < anim.layerCount; i++)
                        {
                            anim.SetLayerWeight(i, 0);
                        }
                        anim.Play("Touch" + Random.Range(0, TC));
                        finish = false;
                    }
                }
            }
        }

    }
}
