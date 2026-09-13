using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
public class WaterWave : MonoBehaviour
{
    public SpriteShapeController ssc;
    public Vector3 PointA;
    public Vector3 PointB;
    public List<Vector3 >allPoints = new();//1.Position.X 2.Position.Y 3.Height 4.Time
    public List<Vector3> Curve = new();
    public float Strongth;
    public int CheckTimes;
    public float Resistance = 12f;

    int u = 0;
    private void OnBecameInvisible()
    {
        Visible = false;
    }

    private void OnBecameVisible()
    {
        Visible = true;
    }
    public bool Visible = false;

    public void AddNewPoint(Vector3 pos,float Strength)
    {
        if (Mathf.Abs( pos.y - PointA.y - transform.position.y) < 0.1f)
        {
            allPoints.Insert(0,new(pos.x, Strength, Time.time));
        }
    }
    private void FixedUpdate()
    {
        if (!Visible) return;
        u++;
        if (u < CheckTimes * Time.timeScale) return;
        u = 0;
        bool Catch = false;
        RaycastHit2D[] rhl = Physics2D.LinecastAll(Pos + PointA, Pos + PointB);
        List<Vector3> firstList = new();
        foreach (RaycastHit2D rh in rhl)
        {
            if (rh.collider.gameObject.CompareTag("Ground")) continue;
            if (rh.collider.isTrigger) continue;
            Catch = true;
            float A = PointA.x + Pos.x;
            //float X = OriginCurve.Find(s => Mathf.Abs(s.x - (rh.point.x - PointA.x - Pos.x)) <= PointDistance / 2f).x + PointA.x + Pos.x;
            float X = OriginCurve[(int)((1-(rh.point.x - A) / ABlength) * PointNum)].x + A; ;
            Vector3 add = new(X, -Mathf.Sqrt((rh.rigidbody.velocity * new Vector2(0.2f, 1f)).magnitude) * Time.fixedDeltaTime * Strongth, Time.time);
            int i = firstList.FindIndex(s => s.x == X);
            if (i >= 0)
            {
                firstList[i] += new Vector3(0, add.y, 0);
            }
            else
            {
                firstList.Add(add);
            }
        }
        allPoints.AddRange(firstList);
        if (!Catch)
        {
            u = (int)CheckTimes - 2;
        }
    }
    public float LoseSpeed = 0;
    public float 波动频率 = 20;
    public float 传播速度 = 35;
    float ABlength;
    public int maxPointers;
    List<Vector3> OriginCurve = new();
    Vector3 Pos;
    private void Awake()
    {
        ABlength = (PointB - PointA).x;
        PointNum = (int)(ABlength / PointDistance);
        for (int i = 0; i < PointNum; i++)
        {
            float X = ABlength / (PointNum + 1);
            OriginCurve.Insert(0,new(X * (i + 1), 0));
        }
        Curve = new();
        Curve.AddRange(OriginCurve);
        Pos = transform.position;
    }
    private void Update()
    {
        if (!Visible) return;
        if(allPoints.Count > maxPointers * 10) allPoints.RemoveRange(0, allPoints.Count - maxPointers * 10);
        Parallel.For(0, Curve.Count, x =>
         {
             Curve[x] = OriginCurve[x];
         });
        List<float> DTL = new();
        for (int a = allPoints.Count -1; a > -1; a--)
        {
            float DT = Time.time - allPoints[a][2];
            if (DT > 1 / LoseSpeed)
            {
                allPoints.RemoveAt(a);
            }
            else
            {
                DTL.Insert(0, DT);
            }
        }
        Parallel.For(0, allPoints.Count, i =>
          {
              DrawCurve(allPoints[i][0], DTL[i], allPoints[i][1]);
          });
        //CutCurve();

        Show();
    }
    public float PointDistance;

    int PointNum;
    public float IgnoreHeight;
    void CutCurve()
    {
        int state = 0;
        List<int> Remo = new();
        bool RR = false;
        for (int i = 1; i < Curve.Count - 1; i++)
        {
            if (Mathf.Abs(Curve[i].y) > IgnoreHeight)
            {
                if (Curve[i].y > Curve[i + 1].y)
                {
                    if (state == -1)
                    {
                        if (RR)
                        {
                            RR = false;
                        }
                        else
                        {
                            Remo.Add(i);
                        }
                    }
                    else
                    {
                        RR = true;
                    }
                    state = -1;
                }
                if (Curve[i].y < Curve[i + 1].y)
                {
                    if (state == 1)
                    {
                        if (RR)
                        {
                            RR = false;
                        }
                        else
                        {
                            Remo.Add(i);
                        }
                    }
                    else
                    {
                        RR = true;
                    }
                    state = 1;
                }
            }
            else
            {
                Remo.Add(i);
            }
        }
        for (int i = Remo.Count -1; i > -1; i--)
        {
            Curve.RemoveAt(Remo[i]);
        }
        if (Curve.Count != 0)
        {
            Curve.RemoveAt(Curve.Count - 1);
            Vector3 p = Curve[0];
            p.y = 0;
            Curve[0] = p;
            p = Curve[Curve.Count - 1];
            p.y = 0;
            Curve[Curve.Count - 1] = p;
        }
    }
    void DrawCurve(float Center, float time,float Height)
    {
        for (int i = 0; i < PointNum; i++)
        {
            float X = (PointB - PointA).x / (PointNum + 1);
            float XX = X * (i + 1) - (Center - Pos.x - PointA.x);
            if (XX < 0)
            {
                float GX = (XX + time * 传播速度) / -波动频率 + 1;
                GX = GX < 0 || GX > 1 ? 0 : GX;
                GX -= LoseSpeed * time;
                GX = GX < 0 || GX > 1 ? 0 : GX;
                Vector3 add = new Vector3(X * (i + 1), Height * GX * Mathf.Cos(XX + time * 传播速度 - 3.14f * 2.4f), 0);
                Curve[PointNum - i - 1] += new Vector3(0, add.y, 0);
            }
            else
            {
                float GX = (XX - time * 传播速度) / 波动频率 + 1;
                GX = GX < 0 || GX > 1 ? 0 : GX;
                GX -= LoseSpeed * time;
                GX = GX < 0 || GX > 1 ? 0 : GX;
                Vector3 add = new Vector3(X * (i + 1), Height * GX * Mathf.Cos(XX - time * 传播速度 + 3.14f * 2.4f), 0);
                Curve[PointNum - i - 1] += new Vector3(0, add.y, 0);
            }
        }
    }
    void Show()
    {
        int A = 0;
        int B = 0;
        for (int i = 0; i < ssc.spline.GetPointCount(); i++)
        {
            if (ssc.spline.GetPosition(i) == PointA)
            {
                A = i;
            }
            if (ssc.spline.GetPosition(i) == PointB)
            {
                B = i;
            }
        }
        for (int i = 0; i < B - A -1; i++)
        {
            ssc.spline.RemovePointAt(A + 1);
        }
        for (int i = 0; i < Curve.Count; i++)
        {
            ssc.spline.InsertPointAt(A + 1, Curve[i] +new Vector3( PointA.x,PointA.y));
        }
    }
}
