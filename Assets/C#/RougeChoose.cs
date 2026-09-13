using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RougeChoose : MonoBehaviour
{
    public string[] StartList;
    public string[] LevelList;
    public string[] RestList;
    public string[] BossList;


    public GameObject Choose;
    public GameObject LevelManager;

    public YourSave YS;

    public FinishCharacter fc;
    private void Start()
    {
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        if (File.Exists(path))
        {
            LoadMap();
        }
    }
    private void OnValidate()
    {
        if (PassLevel)
        {
            SettlementReward();
            PassLevel = false;
        }
    }
    public void LoadMap()
    {
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        string json = File.ReadAllText(path);
        var test = JsonUtility.FromJson<YourSave>(json);
        YS = test;

        allScene = new();
        bool next = true;
        for (int i = 0; i < YS.SceneList.Length; i++)
        {
            if (next)
            {
                next = false;
                allScene.Add(new List<string>() { });
            }
            if (YS.SceneList[i] == "Next")
            {
                next = true;
            }
            else
            {
                allScene[^1].Add(YS.SceneList[i]);
            }
        }
        Choose.SetActive(false);
        print(YS.RightNum);
        if (float.Parse(allScene[0][4]) <= 0 || YS.RightNum == 8)
        {
            SettlementReward();
        }
        else
        {
            LevelManager.SetActive(true);
            LevelManager.GetComponent<LevelManager>().SetMap(allScene);
        }
    }

    public GameObject Settlement;
    void SettlementReward()
    {
        Settlement.SetActive(true);
        int g = YS.RightNum;
        float AllGoldCoins = g * 100 + g * (g - 1) * 50f;
        bool Pass = false;
        if (YS.RightNum == 8 || PassLevel)
        { //过关
            AllGoldCoins += 500;
            Pass = true;
        }
        fc.Show("Anim/" + allScene[0][5],Pass);
        Delete();
    }
    public bool PassLevel;

    public void Delete()
    {
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        File.Delete(path);
        print("Delete");
    }
    public void GetSave(YourSave ys)
    {
        YS = ys;
        SummonMap();
        Choose.SetActive(false);
        LevelManager.SetActive(true);
        LevelManager.GetComponent<LevelManager>().SetMap(allScene);
    }






    /// 1 - 2 ; 2 - 3; 2 - 4; 3 - 2 ; 3 - 4; 4 - 3 ;4 - 2 ;最后合并为1
    /// 岔路口链接上下上下
    /// 休息关在偶数关刷新；商店关在3的倍数关刷新
    /// 存档开头第一Next前记录了 上一关，是否通关，等信息
    public List<List<string>> allScene;
    private void SummonMap()
    {
        allScene = new() { new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { } };
        List<string> line = new();

        allScene[0].Add("-1");
        allScene[0].Add("0");
        allScene[0].Add("");//如果过关则“Pass”
        allScene[0].Add("0");//银币数
        allScene[0].Add("5");//剩余重开数
        allScene[0].Add(PlayerPrefs.GetString("animPath","BadPig"));//角色example BadPig BoxCat
        for (int i = 0; i < allScene[0].Count; i++)
        {
            line.Add(allScene[0][i]);
        }
        line.Add("Next");

        allScene[1].Add(StartList[Random.Range(0, StartList.Length)]);
        line.Add(allScene[1][0]);
        line.Add("Next");
        for (int i = 2; i < allScene.Count; i++)
        {
            if (i % 5 != 0)
            {
                int lastCount = allScene[i - 1].Count;
                int randCount = lastCount switch
                {
                    1 => Random.Range(2, 3),
                    _ => Random.Range(2, 5),
                };
                bool Rest = (((i + 1f) / 2f) - (int)((i + 1f) / 2f)) == 0;
                for (int c = 0; c < randCount; c++)
                {
                    if (Rest)
                    {
                        if (randCount - 1 == c || Random.Range(0, 2) == 0)
                        {
                            allScene[i].Add(RestList[Random.Range(0, RestList.Length)]);
                            line.Add(allScene[i][c]);
                            Rest = false;
                            continue;
                        }
                    }
                    allScene[i].Add(LevelList[Random.Range(0, LevelList.Length)]);
                    line.Add(allScene[i][c]);
                }
                line.Add("Next");
            }
            else
            {
                allScene[i].Add(BossList[Random.Range(0, BossList.Length)]);
                line.Add(allScene[i][0]);
            }
        }

        YS.SceneList = new string[line.Count];
        for (int i = 0; i < line.Count; i++)
        {
            YS.SceneList[i] = line[i];
        }
        YS.RightNum = -1;
        YS.DownNum = 0;

        SaveMap();
    }

    public void SaveMap()
    {
        var json = JsonUtility.ToJson(YS, true);
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        File.WriteAllText(path, json);
    }
}
