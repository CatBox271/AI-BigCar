using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSplitColorControll : MonoBehaviour
{
    public bool flip;
    public Color col;
    public float Brightness = 1;
    public Material mat;
    private Material _mat;
    private SpriteRenderer sp;

    private void Awake()
    {
        TryGetComponent(out sp);
        SetMat();
    }
    private void OnValidate()
    {
        Refresh();
    }
    private void SetMat()
    {
        if(sp == null) TryGetComponent(out sp);
        if (_mat == null) _mat = sp.sharedMaterial = new Material(mat);
    }
    public void Refresh()
    {
        SetMat();
        _mat.SetColor("_Color", col);
        _mat.SetFloat("_Mirror", flip ? 1 : 0);
        _mat.SetFloat("_Brightness", Brightness);
    }
}
