using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RandomUD : MonoBehaviour
{
    public SpriteShapeController ssc;
    public bool Refresh;
    public float Limit;
    public bool On;
    private void Awake()
    {
        if (On)
        {
            RandomLoad();
        }
    }
    private void OnValidate()
    {
        if (Refresh)
        {
            RandomLoad();
            Refresh = false;
        }
    }
    public int AllPointCount;
    public void RandomLoad()
    {
        Vector3 EndPos = ssc.spline.GetPosition(2);
        Vector3 StartPos = ssc.spline.GetPosition(1);
        float AllDis = EndPos.x - StartPos.x;
        float EachDis = AllDis /( AllPointCount + 1);
        for (int i = 0; i < AllPointCount; i++)
        {
            ssc.spline.InsertPointAt(2, new Vector3(EndPos.x, 0) - new Vector3(EachDis,- StartPos.y- Random.Range(0, Limit)));
            ssc.spline.SetTangentMode(2, ShapeTangentMode.Continuous);
            EndPos = ssc.spline.GetPosition(2);
        }
        for (int i = 0; i < AllPointCount; i++)
        {
            ssc.spline.SetLeftTangent(2 + i, new Vector3( EachDis / -2.5f,0));
            ssc.spline.SetRightTangent(2 + i, new Vector3(EachDis / 2.5f, 0));
        }
    }
}
