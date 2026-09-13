using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroscopeAnimation : MonoBehaviour
{
    float r;
    float aim_R;
    public Groscope gro;
    private void Awake()
    {
        StartCoroutine(anim());
    }

    private void Update()
    {
        if (gro.On)
        {
            r = Mathf.Lerp(r, aim_R, 0.01f);
            transform.eulerAngles += new Vector3(0, 0, r * Time.deltaTime);
        }
    }
    IEnumerator anim()
    {
        do
        {
            aim_R = Random.Range(90f, 360f) * (Random.Range(0, 2) - 0.5f) * 2f;
            yield return new WaitForSeconds(Random.Range(5f, 20f));
        } while (true);
    }
}
