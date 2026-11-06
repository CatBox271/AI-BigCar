
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TryConnect : MonoBehaviour
{
    public bool[] CantConnect = new bool[5];
    public List<Transform> Object = new ();
    //1至5分别代表 上 下 左 右 中

    public bool Father;

    public Rigidbody2D rb_1;
    public Rigidbody2D rb_2;

    public float GetSpeed = 0;

    public List<RelativeJoint2D> ImportJoint = new();
    public bool Center;

    public bool Inside;
    public bool Rotable;
    public bool LayerCross;

    public float addPercent = 1f;

    public float ConnectStrongth = 1500f;

    public int forward;

    public int layIn = 0;

    private void Awake()
    {
        if (SPL.Count == 0)
        {
            SPL.AddRange(gameObject.GetComponentsInChildren<SpriteRenderer>());
            SPL.Add(GetComponent<SpriteRenderer>());
        }
    }
    public void SetStart(int camp)
    {
        if (rb_1 != null)
        {

            rb_1.isKinematic = false;
            rb_1.simulated = true;
            rb_1.freezeRotation = false;
            rb_1.velocity = Vector2.zero;
            rb_1.drag = 0f;
            rb_1.angularVelocity = 0;
            Heath h = rb_1.GetComponent<Heath>();
            h.IgnoteCampCollider = layIn != 0;
            h.Set(camp);
            rb_1.gameObject.AddComponent<AirResistance>().rb = rb_1;
        }
        if (rb_2 != null)
        {
            rb_2.isKinematic = false;
            rb_2.simulated = true;
            rb_2.freezeRotation = false;
            rb_2.velocity = Vector2.zero;
            rb_2.drag = 0f;
            rb_2.angularVelocity = 0;
            Heath h = rb_2.GetComponent<Heath>();
            h.IgnoteCampCollider = layIn != 0;
            h.Set(camp);
            rb_2.gameObject.AddComponent<AirResistance>().rb = rb_2;
        }
    }

    List<SpriteRenderer> SPL = new();
    bool low;
    bool layerSet;
    public void LayInShow()
    {
        if (!LayerCross)
        {
            if (layIn == GameManager.layIn)
            {
                if (low)
                {
                    low = false;
                    SPL.ForEach(sp => sp.color *= 1.75f);
                }

            }
            else
            {
                if (!low)
                {
                    low = true;
                    SPL.ForEach(sp => sp.color *= 0.57142857f);
                }
            }
        }
        RenderSort(LayerCross);
    }
    void RenderSort(bool f =false)
    {
        if (!layerSet)
        {
            layerSet = true;
            SPL.ForEach(sp => sp.sortingOrder += f ? 1000 : (layIn * 1000));
        }
    }
    public void BackColor()
    {
        if (!LayerCross)
        {
            if (low)
            {
                SPL.ForEach(sp => sp.color *= 1.75f);
            }
        }
        RenderSort(LayerCross);
    }
}
