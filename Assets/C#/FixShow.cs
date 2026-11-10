using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixShow : MonoBehaviour
{
    private Txt t;
    private void Awake()
    {
        TryGetComponent(out t);
        Destroy(gameObject, 3f);
        StartCoroutine("anim");
    }

    IEnumerator anim()
    {
        Vector3 startSpeed = new(0f,1f);
        do
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out SpriteRenderer sp))
                {
                    sp.color -= Time.deltaTime * new Color(0, 0, 0, 0.1f);
                }
            }
            transform.position += startSpeed * Time.deltaTime;
            startSpeed.y -= Time.deltaTime * 0.3f;
            
            
            
            
            
            
            
            ;
        } while (true);
    }

    public void Setting(float _fix)
    {
        t.color = new Color(0.75f, 1f, 0.75f,0.75f);
        // 间距、水平/竖直偏移都随 size 等比缩放，只需缩放字号
        t.size *= Mathf.Clamp(_fix / 15f, 1f, 3f);
        t.context = ((int)_fix).ToString();
        StartCoroutine("anim");
    }
}
