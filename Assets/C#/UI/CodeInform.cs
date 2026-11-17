using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodeInform : MonoBehaviour
{
    public Text text;
    public Animator anim;//Canvas 上那个 Animator（MoveVehicle）

    public void Show(string kind)//百科打开时才更新说明，没打开就忽略
    {
        if (!anim.GetBool("百科")) return;
        text.text = GameManager.GetCodeDescribe(kind);
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
