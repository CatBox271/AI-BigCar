using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPool : MonoBehaviour
{
    public GameObject Follow;

    public void SetStar(Transform on, int grade)
    {
        GameObject go = Instantiate(Follow, transform);
        go.GetComponent<StarFollow>().SetStar(on, grade);
    }

    public void SetStar(Transform on, int grade, float Size , float Offest)
    {
        GameObject go = Instantiate(Follow, transform);
        go.GetComponent<StarFollow>().SetStar(on, grade);
    }
}
