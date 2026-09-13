using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutPutManager : MonoBehaviour
{
    public void Sort()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tf = transform.GetChild(i);
            tf.localPosition = new(-54 + i % 3 * 54, 190 - i / 3 * 54);
        }
    }

    private void OnEnable()
    {
        Sort();
    }
}
