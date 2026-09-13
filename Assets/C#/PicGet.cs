using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PicGet : MonoBehaviour
{
    public Image image;
    public Sprite NumPic;
    public SwarmCode sc;
    public DragUI dui;

    public void SetPic((string , int) SI)
    {
        if (new List<string>() { "HydraulicHinge", "GeneralCannonAngle" }.Contains(SI.Item1)) image.sprite = NumPic;
        sc.kind = "Item:" + SI.Item1+":" + SI.Item2;
        switch (SI.Item2)
        {
            case 0:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case 1:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case 2:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case 3:
                transform.GetChild(0).transform.localEulerAngles = new Vector3(0, 0, -90);
                break;
        }
        transform.GetChild(0).GetComponent<RawImage>().texture = Resources.Load("Sprite/" + SI.Item1) as Texture;
    }
    private void Start()
    {
        dui.UI_MoveBar = transform.parent.GetComponent<UIMoveBar>();
    }
}
