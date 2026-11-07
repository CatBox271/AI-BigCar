using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCombine : MonoBehaviour
{
    public Transform aim;
    public float blendTime = 3f;
    public float predistance = 10f;
    public AnimationCurve 缓入缓出;
    private float _blendTime = 0;
    public List<SpriteRenderer> sps;
    public List<CameraSplitColorControll> splits;
    private void FixedUpdate()
    {
        if (aim.position.x < transform.position.x + predistance)
        {
            _blendTime += Time.fixedDeltaTime;
            _blendTime = Mathf.Min(_blendTime, blendTime);
        }
        else
        {
            _blendTime -= Time.fixedDeltaTime;
            _blendTime = Mathf.Max(0, _blendTime);
        }
        //离开缓冲
        float rate = 缓入缓出.Evaluate(_blendTime / blendTime);
        OwnF.BlendRate = AimF.BlendRate = rate;

        OwnF.CombinedX = VideoCameraFollow.vcf.MX - VideoCameraFollow.vcf.cameraW;
        AimF.CombinedX = VideoCameraFollow.vcf.MX + VideoCameraFollow.vcf.cameraW;
        //取景框
        float a = rate;
        foreach (var sp in sps)
        {
            Color c = sp.color;
            c.a = a;
            sp.color = c;
        }

        foreach (var split in splits)
        {
            split.Brightness = Mathf.Clamp((aim.position.x - transform.position.x - predistance) / 110f, 0, 2f) * (1 - rate);
            split.Refresh();
        }
    }

    private void Start()
    {
        AimP = aim.parent.parent;
        OwnP = transform.parent.parent;
        AimF = AimP.GetComponent<VideoFollow>();
        OwnF = OwnP.GetComponent<VideoFollow>();
        AimC = AimF._camera;
        OwnC = OwnF._camera;
    }
    Transform AimP, OwnP;
    VideoFollow AimF, OwnF;
    Camera AimC, OwnC;

    private void Combine()
    {
        OwnC.enabled = AimC.enabled = false;
    }
}
