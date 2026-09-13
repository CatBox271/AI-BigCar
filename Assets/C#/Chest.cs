using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    public bool Sliver;
    public int _index;
    public int Money;
    public Animator am;
    public bool c;
    public Text text;
    private void Start()
    {
        if (!Sliver)
        {
            string all = PlayerPrefs.GetString("Chest", "");
            if (all.Contains(_index + "#")) Destroy(gameObject);
        }
    }
    float time;
    private void Update()
    {
        if (GameManager.gameState == GameManager.GameState.Ready)
        {
            time = Time.time;
        }

    }
    float t = 0;
    public float UT;
    private void LateUpdate()
    {
        if (c)
        {
            if (!Sliver)
            {
                t += Time.deltaTime;
                transform.parent = Camera.main.transform;
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, -UT * Camera.main.orthographicSize, 0), t * t);
                transform.localScale = Vector3.Lerp(transform.localPosition, Camera.main.orthographicSize * 0.25f * Vector3.one, t * t);
                transform.localEulerAngles = new Vector3(0, 0, 0);
                if (t > 1 && t < 2)
                {
                    PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + Money);
                    PlayerPrefs.SetString("Chest", PlayerPrefs.GetString("Chest", "") + _index + "#");
                    PlayerPrefs.Save();
                    am.Play("ChestOpen");
                    text.text = "½ð±Ò+" + Money;
                    t = 100;
                }
            }
            else
            {
                if (!Add)
                {
                    Add = true;
                    AddCoin();
                }
                t += Time.deltaTime;
                transform.parent = Camera.main.transform;
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, -UT * Camera.main.orthographicSize, 0), t * t);
                transform.localScale = Vector3.Lerp(transform.localPosition, Camera.main.orthographicSize * 0.25f * Vector3.one, t * t);
                transform.localEulerAngles = new Vector3(0, 0, 0);
                if (t > 1 && t < 2)
                {

                    PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + Money);
                    PlayerPrefs.SetString("Chest", PlayerPrefs.GetString("Chest", "") + _index + "#");
                    PlayerPrefs.Save();
                    am.Play("ChestOpen");
                    text.text = "Ó²±Ò +" + Money;
                    t = 100;
                }
            }
        }
    }
    bool Add;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - time < 0.5f) return;
        if (GameManager.gameState != GameManager.GameState.TurnWin && GameManager.gameState != GameManager.GameState.Win)
        {
            if (collision.TryGetComponent(out Heath h))
            {
                if (h.HP > 0)
                {
                    if (h.camp == 1)
                    {
                        c = true;
                        Destroy(gameObject, 5f);
                        foreach (SpriteRenderer sp in FindSprite(transform))
                        {
                            sp.sortingOrder += 1000;
                        }
                    }
                }
            }
        }
    }

    List<SpriteRenderer> FindSprite(Transform father)
    {
        List<SpriteRenderer> sp = new();
        if (father.TryGetComponent(out SpriteRenderer spp))
        {
            sp.Add(spp);
        }
        for (int i = 0; i < father.childCount; i++)
        {
            sp.AddRange(FindSprite(father.GetChild(i)));
        }
        return sp;
    }

    public void AddCoin()
    {
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        string json;
        if (File.Exists(path))
        {
            json = File.ReadAllText(path);
        }
        else
        {
            Debug.Log("Can't find Map");
            return;
        }
        var test = JsonUtility.FromJson<YourSave>(json);
        test.SceneList[3] = (float.Parse(test.SceneList[3]) + Money).ToString();


        json = JsonUtility.ToJson(test, true);
        path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
        Debug.Log("SaveNew");
        File.WriteAllText(path, json);
    }
}
