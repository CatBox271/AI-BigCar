using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBlock : MonoBehaviour
{
    public Transform Choosed;

    public void Select(string name)
    {
        Choosed = transform.Find(name);
    }
                        //"Artillery" => 1,
                        //"Block" => 2,
                        //"BroadSwordMines" => 3,
                        //"Center" => 4,
                        //"drill" => 5,
                        //"Engine" => 6,
                        //"FixingBag" => 7,
                        //"Groscope" => 8,
                        //"Laser" => 9,
                        //"panzer" => 10,
                        //"PowerProperllers" => 11,
                        //"PowerWheel" => 12,
                        //"RPG" => 13,
                        //"shield" => 14,
                        //"Stealth" => 15,
                        //"TNT" => 16,
                        //"Wheel" => 17,
                        //_ => 1.1f,
    public void Confirm()
    {
        switch (Choosed.name)
        {
            case "装甲流":
                ItemList.AddRange(new float[] { 4, 1, 2, 4, 12, 4, 1, 1, 10, 4, 6, 1 });
                break;
            case "速度流":
                ItemList.AddRange(new float[] { 4, 1, 2, 6, 12, 4, 6, 3, 5, 2 });
                break;
            case "赌狗流":
                ItemList.AddRange(new float[] { 4, 1, 2, 4, 12, 2,1,1});
                for (int i = 0; i < 6; i++)
                {
                    int R;
                    do
                    {
                        R = Random.Range(1, GameManager.BlockIndex.Count);
                    } while (R == 4 || R == 1 || R == 12 || R == 2);
                    bool Find = false;
                    for (int n = 0; n < ItemList.Count; n+= 2)
                    {
                        if (ItemList[n] == R)
                        {
                            ItemList[n + 1]++;
                            Find = true;
                            break;
                        }
                    }
                    if (!Find)
                    {
                        ItemList.AddRange(new float[] { R, 1 });
                    }
                }
                break;
        }
        SaveMap();
    }
    public List<float> ItemList = new();
    public void SaveMap()
    {
        var test = new YourSave();
        test.block_save = new string[ItemList.Count];
        for (int i = 0; i < ItemList.Count; i++)
        {
            test.block_save[i] = ItemList[i].ToString();
        }
        transform.parent.parent.GetComponent<RougeChoose>().GetSave(test);
    }
    private void Update()
    {
        int num = Choosed.GetSiblingIndex();
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 v3 = new();
            if (num != i)
            {
                v3 = new Vector3((i - num) * Screen.width/2.5f, Screen.height / 2f - Screen.height / 4f);
            }
            else
            {
                v3 = new Vector3(Screen.width / -2f +  Screen.width / 5f, Screen.height / 2f - Screen.height / 4f);
            }
            CorrectUI cu = transform.GetChild(i).GetComponent<CorrectUI>();
            cu.SizePercentage = Mathf.Lerp(cu.SizePercentage,Mathf.Abs(i - num) * -10f + 40f, Time.deltaTime * 5f);
            transform.GetChild(i).GetComponent<CanvasGroup>().alpha = cu.SizePercentage / 40f;
            transform.GetChild(i).localPosition = Vector3.Lerp(transform.GetChild(i).localPosition, v3, Time.deltaTime * 5f);
            transform.GetChild(i).localEulerAngles = new Vector3(0, Mathf.Lerp(transform.GetChild(i).localEulerAngles.y, (i - num) * 45f + ((i - num) < 0 ? 360 : 0), Time.deltaTime * 5f), 0);
        }
    }

}
