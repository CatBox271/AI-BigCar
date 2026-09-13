using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnToLevel : MonoBehaviour
{
    public int Level;
    public Text txt1;
    public Text txt2;
    public Image image;
    private void Start()
    {
        int Y = (int)(Level / 7f);
        int X = Level - 7 * Y;
        transform.localPosition = new Vector3(-600, 300) + new Vector3(X * 200, Y * -200);
        txt1.text = (Level + 1).ToString();
        txt2.text = (Level + 1).ToString();
        int get = PlayerPrefs.GetInt("LevelUnLock", 3);
        if (Level < get)
        {
            Level++;
            Instantiate(gameObject, transform.parent);
            Level--;
            image.color = new Color(0, 0.75f, 0);
        }
    }
    public void TurnLevel()
    {
        string nn = "Level" +Level;
        SceneManager.LoadScene(nn);
    }
}
