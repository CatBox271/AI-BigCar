using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VideoFollow : MonoBehaviour
{
    public bool Red;

    private void LateUpdate()
    {
        Move();
    }

    public float AimX = 0;
    public float IX = 0;
    public float V;
    Vector3 T;
    void Move()
    {
        float R = 0; float B = 0;
        if (VideoCameraFollow.vcf.RF != null) R = VideoCameraFollow.vcf.RF.position.x - -110;
        if (VideoCameraFollow.vcf.BF != null) B = 110 - VideoCameraFollow.vcf.BF.position.x;
        if (Red)
        {
            AimX = (R - 110);
        }
        else
        {
            AimX = (110 - B);
        }
        T = transform.position;
        IX = Mathf.Lerp(IX, AimX, V);
        T.x = Mathf.Lerp(T.x, IX, V);
        transform.position = T;
    }
}
