using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    public float PosAdd;
    public RougeChoose rc;
    bool Down;
    float MX;
    public List<List<LevelKind>> LLK = new();
    public void SetMap(List<List<string>> all)
    {
        LLK = new();
        for (int i = 0; i < all.Count-1; i++)
        {
            LLK.Add(new List<LevelKind> { });
            for (int a = 0; a < all[i + 1].Count; a++)
            {
                GameObject go = Instantiate(transform.GetChild(0).gameObject, transform);
                LevelKind LK = go.GetComponent<LevelKind>();
                LK.SetPos = new Vector3(i * 400f, 150f * all[i + 1].Count / 2f - 75f -160f * a);
                LK.SceneName = all[i + 1][a];

                LK.transform.GetChild(0).GetComponent<Text>().text = all[i + 1][a] switch
                {
                    "Start 1" => "初遇",
                    "Speed" => "小山丘",
                    "Speed 1" => "“登山”",
                    "Fight 1" => "泥头车",
                    "Fight" => "大型泥头车",
                    _ => all[i + 1][a],
                };
                LK.GetComponent<Image>().color = all[i + 1][a] switch
                {
                    "Start 1" => Color.white,
                    "Speed" => Color.cyan,
                    "Speed 1" => Color.cyan,
                    "Fight 1" => Color.yellow,
                    "Fight" => Color.red,
                    _ => Color.white,
                };
                LLK[i].Add(LK);
            }
        }
        List<Transform> tf = new();
        for (int b = 0; b < LLK[1].Count; b++)
        {
            tf.Add(LLK[1][b].transform);
        }
        LLK[0][0].SetLine(tf);
        for (int i = 1; i < LLK.Count - 2; i++)
        {
            for (int a = 0; a < LLK[i].Count; a++)
            {
                if (LLK[i + 1].Count == 1)
                {
                    for (int b = 0; b < LLK[i].Count; b++)
                    {
                        LLK[i][b].SetLine(new List<Transform>() { LLK[i+1][0].transform });
                    }
                }
                else
                {
                    tf = new();
                    if (LLK[i].Count < LLK[i + 1].Count)
                    {
                        if (LLK[i].Count == 2 && LLK[i + 1].Count == 4)
                        {
                            tf.Add(LLK[i + 1][2 * a].transform);
                            tf.Add(LLK[i + 1][2 * a + 1].transform);
                        }
                        else
                        {
                            tf.Add(LLK[i + 1][a].transform);
                            tf.Add(LLK[i + 1][a + 1].transform);
                        }
                    }
                    else
                    {
                        if (LLK[i].Count == LLK[i + 1].Count)
                        {
                            tf.Add(LLK[i + 1][a].transform);
                            if (a + 1 != LLK[i + 1].Count)
                            {
                                tf.Add(LLK[i + 1][a + 1].transform);
                            }
                        }
                        else
                        {
                            if (LLK[i].Count == 4 && LLK[i + 1].Count == 2)
                            {
                                tf.Add(LLK[i + 1][(int)(a / 2f)].transform);
                            }
                            else
                            {
                                if (a - 1 != -1)
                                {
                                    tf.Add(LLK[i + 1][a - 1].transform);
                                }
                                if (a != LLK[i + 1].Count)
                                {
                                    tf.Add(LLK[i + 1][a].transform);
                                }
                            }
                        }
                    }
                    LLK[i][a].SetLine(tf);
                }
            }
        }
        for (int b = 0; b < LLK[^2].Count; b++)
        {
            LLK[^2][b].SetLine(new List<Transform>() { LLK[^1][0].transform });
        }
        print(rc.YS.RightNum + "," + rc.YS.DownNum);
        if (rc.YS.SceneList[2] == "Pass")
        {
            LLK[rc.YS.RightNum][rc.YS.DownNum].GetComponent<LevelKind>().Choosed();

            rc.YS.SceneList[0] = rc.YS.RightNum.ToString();
            rc.YS.SceneList[1] = rc.YS.DownNum.ToString();
            rc.YS.SceneList[2] = "";
        }
        else
        {
            print("String Out");
            foreach (string ss in rc.YS.SceneList)
            {
                print(ss);
            }
            if ((int)float.Parse(rc.YS.SceneList[0]) > 0)
            {
                LLK[(int)float.Parse(rc.YS.SceneList[0])][(int)float.Parse(rc.YS.SceneList[1])].GetComponent<LevelKind>().Choosed();
            }
            else
            {
                LLK[0][0].GetComponent<LevelKind>().BeStart();
            }
        }
        transform.GetChild(0).gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Down = true;
            MX = Input.mousePosition.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Down = false;
        }
        if (Down)
        {
            PosAdd += (Input.mousePosition.x - MX) / transform.lossyScale.x;
            MX = Input.mousePosition.x;
        }
    }
}
