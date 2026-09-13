using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 车辆样板加载 : MonoBehaviour
{
    public OwnCar oc;
    private void OnValidate()
    {
        TryGetComponent(out oc);
    }

    private void Start()
    {
        Invoke(nameof(firstcar), 0.25f);
    }

    void firstcar()
    {
        SetCarState(4);
        oc.SendCar = true;
    }

    public List<string> car1;
    public List<string> car2;
    public List<string> car3;
    public List<string> car4;
    public List<string> car5;

    public void SetCarState(int state)
    {
        //赋予预设
        switch (state)
        {
            case 0:
                oc.origin_save = car1;
                break;
            case 1:
                oc.origin_save = car2;
                break;
            case 2:
                oc.origin_save = car3;
                break;
            case 3:
                oc.origin_save = car4;
                break;
            case 4:
                oc.origin_save = car5;
                break;
        }
        //然后生成
        oc.RefreshCar();
    }
}
