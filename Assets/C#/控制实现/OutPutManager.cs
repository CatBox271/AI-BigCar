using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutPutManager : MonoBehaviour
{
    public Transform StartPoint;//动态输出块从这个点开始排

    public void Sort()
    {
        if (StartPoint == null) return;
        int k = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tf = transform.GetChild(i);
            if (!tf.TryGetComponent(out PicGet _)) continue;//只排动态输出块，常驻指令块按 Canvas 里摆的位置
            tf.localPosition = StartPoint.localPosition + new Vector3(k % 3 * 54, -k / 3 * 54);
            k++;
        }
    }

    private void OnEnable()
    {
        Sort();
    }
}
