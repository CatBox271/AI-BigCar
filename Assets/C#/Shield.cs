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
    private FlashEffect flash;
    // Start is called before the first frame update
    void Start()
    {
        shield.SetActive(false);
        TryGetComponent(out h);
        sp_s = shield.GetComponent<SpriteRenderer>();
        flash = GetComponent<FlashEffect>();
        if (flash == null) flash = gameObject.AddComponent<FlashEffect>();
        flash.sp = sp_s;
    }

    // Update is called once per frame
    void Update()
    {

        shield.transform.localPosition = new Vector3(0, 0, 0);
        // 底色（含护盾值下降的变暗）每帧刷新给 FlashEffect，由它统一持有 sp_s.color。
        // ordinary_value 在 work 置位前为 0，直接相除会得到 Infinity，
        // 而 FlashEffect 里的 Color.Lerp 遇到 Infinity 会算出 NaN，故先兜底。
        float ratio = ordinary_value != 0f ? shield_value / ordinary_value : 1f;
        Color baseColor = Color.white * Mathf.Max(0.4f, ratio);
        if (flash != null) flash.SetOriginColor(baseColor);
        else sp_s.color = baseColor;
        if (h.camp == 0) return;
        if (!work)
        {
            work = true;
            shield.layer = gameObject.layer;
            ordinary_value = shield_value;
            StopAllCoroutines();
            StartCoroutine("On");
        }
        if (shield_value < 0)
        {
            if (duringTime < 0)
            {
                duringTime = restTime;
                StopAllCoroutines();
                StartCoroutine("Off");
            }
            duringTime -= Time.deltaTime;
            if (duringTime < 0)
            {
                shield_value = ordinary_value;
                StopAllCoroutines();
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
        if (flash != null)
        {
            flash.WhileBeHit();
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


    public GameObject showHit;
}
