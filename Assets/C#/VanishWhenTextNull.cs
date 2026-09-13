using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VanishWhenTextNull : MonoBehaviour
{
    private void Start()
    {
        if (TryGetComponent(out Text txt))
        {
            if (txt.text != "")
            {
                return;
            }
        }
        Destroy(gameObject);
    }
}
