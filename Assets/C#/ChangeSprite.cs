using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    public SwarmAnim sa;
    public List<string> allUnlock = new();
    public int _index;
    private void Awake()
    {
        string get = PlayerPrefs.GetString("UnLockSprite", "BadPig#");
        string now = PlayerPrefs.GetString("animPath", "BadPig");
        do
        {
            int ii = get.IndexOf("#");
            if (ii != -1)
            {
                allUnlock.Add(get.Substring(0, ii));
                get = get.Remove(0, ii + 1);
            }
        } while (get.IndexOf("#") != -1);
        _index = allUnlock.IndexOf(now);
    }


    float lt;
    private void OnMouseDown()
    {
        lt = Time.time;
    }
    private void OnMouseUp()
    {
        if (Time.time - lt < 0.2f)
        {
            if ((Input.mousePosition.x - Screen.width / 2f) > 0)
            {
                Change(false);
            }
            else
            {
                Change(true);
            }
        }
    }

    public void Change(bool Left)
    {
        _index += Left ? -1 : 1;
        if (_index < 0) _index = allUnlock.Count -1; 
        if (_index > allUnlock.Count - 1) _index = 0; 
        sa.path = "Anim/"+allUnlock[_index];
        Destroy(sa.transform.GetChild(0).gameObject);
        PlayerPrefs.SetString("animPath", allUnlock[_index]);
        PlayerPrefs.Save();
    }
}
