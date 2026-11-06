using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VideoFollow : MonoBehaviour
{
    public bool Red;
    public Camera _camera;
    public List<SpriteRenderer> sps;
    private void FixedUpdate()
    {
        Move();
        SetAlpha();
    }

    private void SetAlpha()
    {
        float a = 1 - BlendRate;
        foreach (var sp in sps)
        {
            Color c = sp.color;
            c.a = a;
            sp.color = c;
        }
    }

    public float AimX = 0;
    public float IX = 0;
    public float V;
    Vector3 T;

    public float BlendRate = 0;
    public float CombinedX = 0;
    void Move()
    {
        if (Red) AimX = VideoCameraFollow.vcf.RX;
        else AimX = VideoCameraFollow.vcf.BX;

        T = transform.position;
        IX = Mathf.Lerp(IX, AimX, V);
        T.x = Mathf.Lerp(T.x, IX, V);

        transform.position = T;
        float X1 = Mathf.Lerp(transform.position.x, _camera.transform.position.x, BlendRate);
        _camera.transform.position = new Vector3(Mathf.Lerp(X1, CombinedX, BlendRate), transform.position.y, transform.position.z);
    }
}
