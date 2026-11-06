using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionConnect : MonoBehaviour
{
    public List<GameObject> AdditialConnect = new();
    public List<SpriteRenderer> SpriteList = new();
    SpriteRenderer Original;
    TryConnect ty;
    private void Awake()
    {
        Original = GetComponent<SpriteRenderer>();
        ty = GetComponent<TryConnect>();
    }
    private void Swarm(int towards) 
    {
        GameObject go = new("Stand");
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0, 0.72f, 0);
        go.transform.localScale = Vector3.one;
        AdditialConnect.Add(go);
        GameObject sp = new("Sprite");
        sp.transform.parent = go.transform;
        sp.transform.localPosition = new Vector3(0, -0.72f, 0);
        sp.transform.localScale = Vector3.one;
        SpriteRenderer spp = sp.AddComponent<SpriteRenderer>();
        SpriteList.Add(spp);
        spp.sprite = Original.sprite;
        spp.sortingOrder = Original.sortingOrder;
        switch (towards)
        {
            case 0:
                go.transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case 1:
                go.transform.eulerAngles = new Vector3(0, 0, 180);
                break;
            case 2:
                go.transform.eulerAngles = new Vector3(0, 0, 90);
                break;
            case 3:
                go.transform.eulerAngles = new Vector3(0, 0, -90);
                break;
        }
    }
    void Judge(Vector3 S,Vector3 O ,TryConnect ty)
    {
        float DX = S.x - O.x;
        float DY = S.y -O.y;
        if (Mathf.Abs(DX) > Mathf.Abs(DY))
        {
            if (DX > 0)
            {
                //вС
                if (ty.forward == 3) return;
                Swarm(3);
            }
            else
            {
                //ср
                if (ty.forward == 2) return;
                Swarm(2);
            }
        }
        else
        {
            if (DY > 0)
            {
                //об
                if (ty.forward == 0) return;
                Swarm(0);
            }
            else
            {
                //ио
                if (ty.forward == 1) return;
                Swarm(1);
            }
        }
    }
    public void Refresh()
    {
        for (int i = 0; i < AdditialConnect.Count; i++)
        {
            Destroy(AdditialConnect[i].gameObject);
        }
        AdditialConnect = new();
        SpriteList = new();
        for (int i = 0; i < ty.Object.Count; i++)
        {
            if(ty.Object[i] != null) Judge(transform.position, ty.Object[i].transform.position, ty);
        }
        for (int i = 0; i < ty.ImportJoint.Count; i++)
        {
            if (ty.ImportJoint[i] != null && ty.ImportJoint[i].connectedBody != null) Judge(transform.position, ty.ImportJoint[i].connectedBody.position, ty);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < SpriteList.Count; i++)
        {
            SpriteList[i].color = Original.color;
            SpriteList[i].sortingOrder = Original.sortingOrder;
        }
    }
}
