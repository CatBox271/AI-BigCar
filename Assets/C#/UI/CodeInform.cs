using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodeInform : MonoBehaviour
{
    public Text text;
    public Animator anim;//Canvas 上那个 Animator（MoveVehicle）

    public void Show(string kind)//点哪个指令块就把它的说明显示到代码百科上
    {
        text.text = GameManager.GetCodeDescribe(kind);
        anim.SetBool("百科", true);
    }

    public void Hide()//收起代码百科
    {
        anim.SetBool("百科", false);
    }

    public void Toggle()//百科按钮：开着就收，关着就再展开
    {
        anim.SetBool("百科", !anim.GetBool("百科"));
    }
}
