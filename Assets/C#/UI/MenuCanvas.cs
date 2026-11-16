using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuCanvas : MonoBehaviour
{
    public Animator anim;
    public string State = "Menu";
    string last;
    public void SetState(string state)
    {
        State = state;
    }

    public void CanRouge()
    {
        bool exist = SLManager.Exists("LevelSave", "Rouge");
        print(exist);
        if (exist)
        {
            State = "ReNew";
        }
        else
        {
            Rouge();
        }
    }
    public void Delete()
    {
        SLManager.Delete("LevelSave", "Rouge");
        print("Delete");
    }
    public void Rouge()
    {
        SceneManager.LoadScene("LevelChoose");
    }
    private void Update()
    {
        if (last != State)
        {
            switch (last)
            {
                case "Choose":
                    anim.Play("ChooseDisappear");
                    break;
                case "Menu":
                    anim.Play("MenuDisappear");
                    break;
                case "SandBox":
                    anim.Play("SandBoxDisappear");
                    break;
            }
            switch (State)
            {
                case "Menu":
                    anim.Play("MenuShow");
                    break;
                case "Choose":
                    anim.Play("ChooseShow");
                    break;
                case "ReNew":
                    anim.Play("ReNew");
                    break;
                case "SandBox":
                    anim.Play("SandBoxShow");
                    break;
            }
            last = State;
        }
    }

}
