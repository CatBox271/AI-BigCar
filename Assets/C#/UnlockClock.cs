using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnlockClock : MonoBehaviour
{
    Text t;
    int stage = 0;
    float record_time;
    private void Awake()
    {
        TryGetComponent(out t);
        StartCoroutine(delayDo());
    }
    IEnumerator delayDo()
    {
        stage = 0;
        record_time = Time.time;
        yield return new WaitForSeconds(180);
        stage = 1;
        record_time = Time.time;
        yield return new WaitForSeconds(180);
        stage = 2;
        record_time = Time.time;
        yield return new WaitForSeconds(180);
        stage = 3;
        record_time = Time.time;
        yield return new WaitForSeconds(180);
        stage = 4;
    }
    private void Update()
    {
        switch (stage)
        {
            case 0:
                t.text =
                    "增加：引擎、装甲\n" +
                    "倒计时:" + ((int)(90 - (Time.time - record_time)/2f)).ToString() + "秒";
                break;
            case 1:
                t.text =
                    "增加：×8、激光、螺旋桨；+1框架数\n" +
                    "倒计时:" + ((int)(90 - (Time.time - record_time) / 2f)).ToString() + "秒";
                break;
            case 2:
                t.text =
                    "增加：×16、修理器；+1道具数\n" +
                    "倒计时:" + ((int)(90 - (Time.time - record_time) / 2f)).ToString() + "秒";
                break;
            case 3:
                t.text =
                    "增加：+1框架数、+1武器数\n" +
                    "倒计时:" + ((int)(90 - (Time.time - record_time) / 2f)).ToString() + "秒";
                break;
            default:
                t.text = "";
                break;
        }
    }
}
