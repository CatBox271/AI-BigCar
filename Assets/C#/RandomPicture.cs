using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomPicture : MonoBehaviour
{
    public Image image;
    private void Start()
    {
        Object[] SL = Resources.LoadAll("Sprite/");
        image.sprite = SL[Random.Range(0, SL.Length)] as Sprite;
    }
}
