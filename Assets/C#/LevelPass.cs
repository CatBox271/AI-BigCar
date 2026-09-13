using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPass : MonoBehaviour
{
    public string PathCenter;
    public Animator at;
    bool win;
    public void Update()
    {

        if (GameManager.gameState == GameManager.GameState.Win)
        {
            if (!win)
            {
                PathCenter = BlockManager.bm.animPath;

                FirstWin();


                win = true;
                transform.localScale = Camera.main.orthographicSize * 2f * Vector3.one;
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(true);
                if (BlockManager.bm.Rouge)
                {
                    transform.GetChild(2).gameObject.SetActive(true);
                }
                at.Play("PassLevel");
                GameObject go = Instantiate(Resources.Load(PathCenter) as GameObject, transform.GetChild(1));
                go.transform.localPosition += new Vector3(0, 0.21f, 0);
                go.transform.localScale += new Vector3(0.65f, 0.65f, 0.65f);
                go.transform.localScale /= 1.5f;
                go.GetComponent<Animator>().SetTrigger("Win");
                foreach (SpriteRenderer sp in FindSprite(go.transform))
                {
                    sp.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    sp.sortingOrder += 206;
                }
            }
        }
        else
        {
            at.Play("New State");
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
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

    public void FirstWin()
    {

        BlockManager bm = BlockManager.bm;
        if (!bm.Rouge)
        {
            string n = SceneManager.GetActiveScene().name;
            string all = PlayerPrefs.GetString("PassLevel", "");
            print(all);
            if (!all.Contains(n + "#"))
            {
                all += n + "#";
                PlayerPrefs.SetString("PassLevel", all);
                PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + 100);
                PlayerPrefs.Save();
                print("FW");
            }
        }
    }

}
