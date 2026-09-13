using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FollowMouse : MonoBehaviour
{
    public bool NullBlock;
    private readonly float video_speed = 20f;
    private void Start()
    {
        Video = Camera.main.name == "VideoMode";
        if (Video)
        {
            if (!TryGetComponent(out rb))
            {
                rb = transform.GetChild(0).GetComponent<Rigidbody2D>();
                Rigidbody2D rb_2 = transform.GetChild(1).GetComponent<Rigidbody2D>();
                rb_2.simulated = true;
                rb_2.isKinematic = false;
            }
            rb.simulated = true;
            rb.isKinematic = false;
            rb.velocity = Random.insideUnitCircle.normalized * video_speed;
        }
    }
    Rigidbody2D rb;
    public void MoveAway()
    {
        BlockManager.bm.GetBlock(name, transform);
        Destroy(gameObject);
    }
    public void Moto(Transform tf)
    {
        transform.parent = tf;
        transform.localPosition = new(0, 0, 0);
        OwnCar oc = tf.parent.GetComponent<OwnCar>();
        Vector2Int size = oc.ColliderArea;
        Vector2 v2 = tf.transform.localPosition;

        int _index = (int)((size.x - 1) * 0.5f + v2.x + ((size.y - 1) * 0.5f + v2.y) * size.x);

        oc.AllObject[_index].Add(gameObject);
        Destroy(this);
    }

    public void ClickTo(Transform tf)
    {
        transform.parent = tf;
        OwnCar oc = tf.GetComponent<OwnCar>();
        transform.localPosition = oc.IndexToPos(oc.MouseOn);
        oc.AllObject[oc.MouseOn].Add(gameObject);
        oc.FixedOn(oc.MouseOn);
        Destroy(this);
    }

    public void ClickTo(Transform tf,int to)
    {
        transform.parent = tf;
        OwnCar oc = tf.GetComponent<OwnCar>();
        transform.localPosition = oc.IndexToPos(to);
        oc.AllObject[to].Add(gameObject);
        oc.FixedOn(to,false);
        Destroy(this);
    }
    bool VideoClickTo(Transform tf, int to)
    {
        OwnCar oc = tf.GetComponent<OwnCar>();
        transform.localPosition = oc.IndexToPos(to);
        oc.AllObject[to].Add(gameObject);
        return oc.FixedOn(to, false,true);
    }

    bool Video;
    private void LateUpdate()
    {
        if (Video) return;
        if (Input.touchCount >= 2)
        {
            BlockManager.bm.GetBlock(name, transform);
            Destroy(this);
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (IsOnUIElement(Input.mousePosition) || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height || Input.mousePosition.x < 0 || Input.mousePosition.y < 0)
            {
                MoveAway();
                return;
            }
            
            List<OwnCar> ocl = BlockManager.bm.FindCar();
            bool find = false;
            foreach (OwnCar oc in ocl)
            {
                if (oc.MouseOn > -1 && oc.MouseOn < oc.ColliderArea.x * oc.ColliderArea.y)
                {
                    ClickTo(oc.transform);
                    MV_Manager.GetRecord();
                    find = true;
                    break;
                }
            }
            if(!find) MoveAway();
        }
        else
        {
            Vector3 v3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            v3.z = 0;
            transform.position = v3;
        }
    }
    //下面这段是抄的，能跑
    private EventSystem eventSystem;
    private PointerEventData eventData;
    private bool init = false;
    void Init()
    {
        if (!init)
        {
            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);
            init = true;
        }
    }
    public bool IsOnUIElement(Vector2 pos)
    {
        Init();
        eventData.pressPosition = pos;
        eventData.position = pos;
        List<RaycastResult> list = new();
        EventSystem.current.RaycastAll(eventData, list);


        foreach (var temp in list)
        {
            if (temp.gameObject.layer.Equals(5))
            {
                return true;//Equal(5)中的“5”是指图层第五层UI层
            }
        }
        return false;
    }
    //抄的到这里结束
    bool Stop;

    TryConnect tc;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Stop) return;
        if (!Video) return;
        //先判断碰到的是不是
        if (collision.collider.TryGetComponent(out TryConnect ty) && collision.collider.TryGetComponent(out Rigidbody2D _rb) && _rb.isKinematic)
        {
            if (tc == null) TryGetComponent(out tc);
            bool succuss = false;
            if (!ty.transform.parent.TryGetComponent(out OwnCar oc)) return;

            //摆正角度
            transform.localEulerAngles = new();


            //内部的判断和外部不一样
            if (tc.Inside)
            {
                //内部组件只要判断ty能不能容纳
                if (ty.CantConnect[4]) return;
                
                //需要计算在AllObject的第几项
                int _index = oc.GetIndexOfAllObject(ty.transform.position);
                if (_index == -1) return;

                succuss = VideoClickTo(oc.transform,_index);
            }
            else
            {
                //外部组件要判断的就多了
                //先判断相对位置
                Vector2 aim = ty.transform.position;
                Vector2 now = transform.position;
                int where = -1;
                if (Mathf.Abs(now.x - aim.x) > Mathf.Abs(now.y - aim.y))
                {
                    //左2
                    if (now.x < aim.x)
                    {
                        where = 2;
                    }
                    else//右3
                    {
                        where = 3;
                    }
                }
                else
                {
                    //下1
                    if (now.y < aim.y)
                    {
                        where = 1;
                    }
                    else//上0
                    {
                        where = 0;
                    }
                }
                //有了位置判断能不能连上
                if (ty.CantConnect[where]) return;
                //需要计算在AllObject的第几项
                int _index = oc.GetIndexOfAllObject(ty.transform.position);
                if (_index == -1) return;
                int final = oc.GetIndexOfAllObject(ty.transform.position + where switch
                {
                    0 => new(0, 1),
                    1 => new(0, -1),
                    2 => new(-1, 0),
                    3 => new(1, 0),
                });
                if (final == -1) return;
                //不在同一行
                if (oc.OnOtherSide(_index, final)) return;

                succuss = VideoClickTo(oc.transform, final);
            }
            if (succuss)
            {
                transform.parent = oc.transform;
                //成功固定将rb 设定 isKimat
                rb.velocity = new();
                rb.angularVelocity = new();
                rb.isKinematic = true;
                Stop = true;
                Destroy(this);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (Stop) return;
        if (!Video) return;
        KeepVelocity();
    }
    void KeepVelocity()
    {
        if (rb.isKinematic)
        {
            rb.velocity = new();
            rb.angularVelocity = new();
        }
        else
        {
            rb.velocity = rb.velocity.normalized * video_speed;
        }
    }
}
