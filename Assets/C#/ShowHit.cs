using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShowHit : MonoBehaviour
{
    private Txt t;
    private void Awake()
    {
        TryGetComponent(out t);
        Destroy(gameObject, 2f);

    }

    IEnumerator anim()
    {
        transform.position += new Vector3(0, 1f, 0);
        Vector3 startSpeed = new(Random.Range(-3f, 3f), 3f*(Random.Range(0f, Mathf.Clamp(hit / 200f,2,5)) +  7.5f * Mathf.Clamp(hit/ 200f, 1f, 5f)));
        do
        {

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out SpriteRenderer sp))
                {
                    sp.color -= Time.deltaTime * new Color(0,0,0,0.75f);
                }
            }
            transform.position += startSpeed * Time.deltaTime;
            startSpeed -= new Vector3(0, 60f * Time.deltaTime);
            yield return null;


        } while (true);
    }
    float hit;
    public void Setting(int _camp, float _hit)
    {
        if (_camp == 1)
        {
            t.color = new Color(1f, 0.75f, 0.75f,0.75f);
        }
        if (_camp == 2)
        {
            t.color = new Color(0.75f, 0.75f, 1f,0.75f);
        }
        t.size *= Mathf.Clamp(_hit / 30f, 1f, 3f);
        t.offset *= Mathf.Clamp(_hit / 30f, 1f, 3f);
        t.word_offset *= Mathf.Clamp(_hit / 30f, 1f, 3f);
        t.VerticalOffset *= Mathf.Clamp(_hit / 30f, 1f, 3f);
        t.context = ((int)_hit).ToString();
        hit = _hit;
        StartCoroutine("anim");
    }
}
