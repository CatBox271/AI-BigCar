using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Shield : MonoBehaviour
{
    public GameObject shield;
    private Heath h;
    private bool work;
    public float duringTime;
    public float restTime;
    public float size;
    public float shield_value;
    private float ordinary_value;
    private SpriteRenderer sp_s;
    // Start is called before the first frame update
    void Start()
    {
        shield.SetActive(false);
        TryGetComponent(out h);
        sp_s = shield.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        shield.transform.localPosition = new Vector3(0, 0, 0);
        sp_s.color = Color.white * Mathf.Max(0.4f,shield_value / ordinary_value) + _lightPercent;
        if (h.camp == 0) return;
        if (!work)
        {
            work = true;
            shield.layer = gameObject.layer;
            ordinary_value = shield_value;
            StopAllCoroutines();
            HitAnimFinish = true;
            StartCoroutine("On");
        }
        if (shield_value < 0)
        {
            if (duringTime < 0)
            {
                duringTime = restTime;
                StopAllCoroutines();
                HitAnimFinish = true;
                StartCoroutine("Off");
            }
            duringTime -= Time.deltaTime;
            if (duringTime < 0)
            {
                shield_value = ordinary_value;
                StopAllCoroutines();
                HitAnimFinish = true;
                StartCoroutine("On");
            }
            return;
        }


    }
    public void GetHit(float value,Vector3 vec)
    {
        shield_value -= value;
        if (showHit != null)
        {
            Instantiate(showHit, vec, new Quaternion()).GetComponent<ShowHit>().Setting(h.camp, value);
        }
        if (HitAnimFinish)
        {
            StartCoroutine("HitAnim");
        }
    }
    IEnumerator On()
    {
        shield.transform.localScale = Vector3.zero;
        shield.SetActive(true);
        do
        {
            shield.transform.localScale = Vector3.Lerp(shield.transform.localScale, Vector3.one * size + new Vector3(0.1f, 0.1f, 0.1f), 0.1f);
            yield return new WaitForSeconds(0.01f);
        }
        while (shield.transform.localScale.x < size);
    }

    IEnumerator Off()
    {
        int i = 0;
        do
        {
            shield.transform.localScale = new Vector3(shield.transform.localScale.x / 1.05f, transform.localScale.y / 1.1f, transform.localScale.z);
            yield return new WaitForSeconds(0.01f);
            i++;
        } while (i < 50);
        shield.SetActive(false);
    }


    private bool HitAnimFinish = true;
    private Color _lightPercent;
    public GameObject showHit;

    IEnumerator HitAnim()
    {
        HitAnimFinish = false;

        _lightPercent = new Color(0.4f, 0.4f, 0.4f, 0);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 5; i++)
        {
            _lightPercent -= new Color(0.4f, 0.4f, 0.4f, 0) / 5f;
            yield return null;
        }
        _lightPercent = Color.clear;
        yield return new WaitForSeconds(0.1f);
        HitAnimFinish = true;
    }
}
