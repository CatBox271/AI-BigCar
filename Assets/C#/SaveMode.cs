using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SaveMode : MonoBehaviour
{
    public Text OriginalName;
    public Text FinalName;
    public class VehicleSave
    {
        public string[] carsave;
    }

    public MV_Manager mv;
    public Transform DesignButton;
    public InputField NameInput;
    public Text ShowCarName;
    public int Choosed = -1;

    private void OnEnable()
    {
        var _path = Path.Combine(Application.persistentDataPath, "VehicleDesign");
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
        Show();
        if (mv.StateLoad)
        {

        }
        else
        {
            List<OwnCar> ocl = mv.bm.FindCar();
            List<Vector3> CenterPoint = new();
            for (int i = 0; i < mv.RDR.Count; i++)
            {
                Vector3 v3 = ocl[i].transform.position - new Vector3(ocl[i].ColliderArea.x / 2f, ocl[i].ColliderArea.y / 2f);
                v3 += new Vector3((float.Parse(mv.RDR[i][0]) + float.Parse(mv.RDR[i][1])) / 2f + 1, (float.Parse(mv.RDR[i][2]) + float.Parse(mv.RDR[i][3])) / 2f + 1);
                CenterPoint.Add(v3);
            }
            LevelCamera LC = Camera.main.GetComponent<LevelCamera>();
            int Closet = -1;
            for (int i = 0; i < CenterPoint.Count; i++)
            {
                if (Closet == -1) Closet = i;
                if ((LC.transform.position - CenterPoint[i]).magnitude < (LC.transform.position - CenterPoint[Closet]).magnitude)
                {
                    Closet = i;
                }
            }
            if (Closet != -1)
            {
                LC.AimPos = CenterPoint[Closet];
                LC.RelativePos = new();
                float w = float.Parse(mv.RDR[Closet][1] )- float.Parse(mv.RDR[Closet][0]);
                float h = float.Parse(mv.RDR[Closet][3]) - float.Parse(mv.RDR[Closet][2]);
                LC.AimSize = (h > w ? h / 1.5f : w * Screen.height / Screen.width);
                LC.RelativeSize = 0;
                Choosed = Closet;
            }
        }
    }

    int car_width,car_height;

    List<string> ConfirmCarSave = new();
    public void ConfirmLoad()
    {
        List<OwnCar> ocl = mv.bm.FindCar();
        Vector2Int Size = ocl[Choosed].ColliderArea;
        int XS = Size.x / 2 - car_width / 2;
        int YS = Size.y / 2 - car_height / 2;
        int XE = XS + car_width;
        int YE = YS + car_height;

        //读取所有列表
        List<string> get = mv.bm.KeepLevel();
        int carIndex = 0;

        List<List<string>> allcars = new();
        for (int i = 0; i < get.Count; i++)
        {
            if (allcars.Count <= carIndex)
            {
                allcars.Add(new List<string>());
            }
            if (get[i] == "114514")
            {
                carIndex++;
            }
            else
            {
                allcars[carIndex].Add(get[i]);
            }
        }
        //替换列表
        allcars[Choosed] = new();
        int a = 0;
        for (int i = 0; i < Size.x * Size.y; i++)
        {
            int x = i % Size.x;
            int y = i / Size.x;
            if (XS <= x && x < XE && YS <= y && y < YE)
            {
                allcars[Choosed].Add(ConfirmCarSave[a]);
                a++;
                do
                {
                    if (a >= ConfirmCarSave.Count) break;

                    if (ConfirmCarSave[a] == "StartCode")
                    {
                        for (int p = 0; p < 101; p++)
                        {
                            allcars[Choosed].Add(ConfirmCarSave[a + p]);
                        }
                        a += 101;
                        continue;
                    }
                    if (float.Parse(ConfirmCarSave[a]) >= 0) break;

                    allcars[Choosed].Add(ConfirmCarSave[a]);
                    a++;
                } while (true);
            }
            else
            {
                allcars[Choosed].Add("0");
            }
        }
        //最终整合
        List<string> newsave = new();
        for (int i = 0; i < allcars.Count; i++)
        {
            newsave.AddRange(allcars[i]);
            newsave.Add("114514");
        }
        mv.DoTo(newsave);
        mv.anim.SetTrigger("Cancel");
        mv.anim.SetTrigger("Back");
    }
    public void Load(Transform button)
    {
        int _index = button.GetSiblingIndex();
        string[] files = FileUtils.GetFilesInPersistentFolder("VehicleDesign");

        string json = File.ReadAllText(files[_index]);

        var vs = JsonUtility.FromJson<VehicleSave>(json);

        car_width = (int)float.Parse(vs.carsave[1]);

        int BlockCount = 0;
        ConfirmCarSave = new();
        for (int i = 2; i < vs.carsave.Length; i++)
        {
            if (vs.carsave[i] == "StartCode")
            {
                for (int p = 0; p < 101; p++)
                {
                    ConfirmCarSave.Add(vs.carsave[i + p]);
                }
                i += 100;
                continue;
            }
            if (float.Parse(vs.carsave[i]) >= 0)
            {
                BlockCount++;
            }
            ConfirmCarSave.Add(vs.carsave[i]);
        }
        car_height = BlockCount / car_width;
        //Closet
        bool ok = false;
        List<OwnCar> ocl = mv.bm.FindCar();
        if (Choosed != -1)
        {

            Vector2Int Size = ocl[Choosed].ColliderArea;
            if (Size.x > car_width && Size.y > car_height)
            {
                ok = true;
            }
        }
        if (ok)
        {
            //加动画确认
            ShowCarName.text = "是否加载<" + vs.carsave[0] + ">";
            mv.anim.Play("LoadReplaceConfirm");
        }
        else
        { 
            //加载空间不足提示
        }
    }

    public void Show()
    {
        int p = 1;
        if(NameInput.text.Length == 0)
        {
            NameInput.text = "设计" + p;
        }
        string[] files = FileUtils.GetFilesInPersistentFolder("VehicleDesign");


        if (files != null && files.Length != 0)
        {
            for (int i = DesignButton.transform.childCount - 1; i > -1 + files.Length && i> 0; i--)
            {
                Destroy(DesignButton.GetChild(i).gameObject);
            }
            for (int i = 0; i < files.Length; i++)
            {
                string _name = Path.GetFileName(files[i]);
                _name = _name.Remove(_name.Length - 5);
                if (NameInput.text == _name)
                {
                    p++;
                    NameInput.text = "设计" + p;
                }
                RectTransform go;
                if (DesignButton.transform.childCount - 1 < i)
                {
                     go = Instantiate(DesignButton.GetChild(DesignButton.childCount - 1),DesignButton).GetComponent<RectTransform>();
                }
                else
                {
                    go = DesignButton.GetChild(i).GetComponent<RectTransform>();
                }
                InputField tot = go.transform.GetChild(0).GetComponent<InputField>();
                go.GetComponent<SaveName>().origin = _name;
                tot.text = _name;
                go.anchoredPosition = new Vector2(0, -45f - 80f * i);
            }
            RectTransform DB = DesignButton.GetComponent<RectTransform>();
            DB.sizeDelta = new Vector2(DB.sizeDelta.x, 80f * files.Length);
        }
    }

#if UNITY_EDITOR

    public void Replace()
    {
        string Name = FinalName.text;
        string[] files = FileUtils.GetFilesInPersistentFolder("VehicleDesign");
        VehicleSave vs = new();
        bool Exist = false;
        for (int i = 0; i < files.Length; i++)
        {
            string _name = Path.GetFileName(files[i]);
            _name = _name.Remove(_name.Length - 5);

            if (_name == Name)
            {
                Exist = true;
                break;
            }
        }

        if (!Exist) return;
        
        List<string> Addin = new();//当前的设计
        Addin.Add(Name);
        Addin.Add((float.Parse(mv.RDR[Choosed][1]) - float.Parse(mv.RDR[Choosed][0]) + 1).ToString());
        for (int i = 4; i < mv.RDR[Choosed].Count; i++)
        {
            Addin.Add(mv.RDR[Choosed][i].ToString());
        }

        vs.carsave = new string[Addin.Count];
        for (int i = 0; i < Addin.Count; i++)
        {
            vs.carsave[i] = Addin[i];
        }

        var json = JsonUtility.ToJson(vs, true);
        var path = Path.Combine(Application.persistentDataPath, "VehicleDesign", Name + ".json");

        File.WriteAllText(path, json);

        NameInput.text = "设计1";
        mv.anim.SetTrigger("Back");
        mv.anim.SetTrigger("Cancel");
        print("ReplaceDesign");
        AssetDatabase.Refresh();
    }

    public void ChangeNameSave()
    {
        NameInput.text = "新 " + FinalName.text;
        Save();
    }

    public void Rename(string origin,string rename)
    {
        string Name = origin;
        string[] files = FileUtils.GetFilesInPersistentFolder("VehicleDesign");
        bool Exist = false;
        for (int i = 0; i < files.Length; i++)
        {
            string _name = Path.GetFileName(files[i]);
            _name = _name.Remove(_name.Length - 5);

            if (_name == Name)
            {
                Exist = true;
                break;
            }
        }
        if (!Exist) return;

        var path = Path.Combine(Application.persistentDataPath, "VehicleDesign", Name + ".json");
        var vs = JsonUtility.FromJson<VehicleSave>(File.ReadAllText(path));
        vs.carsave[0] = rename;

        File.Delete(path);
        path = Path.Combine(Application.persistentDataPath, "VehicleDesign", rename + ".json");
        var json = JsonUtility.ToJson(vs, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }
    public void Save()
    {
        string Name = OriginalName.text;
        if (FinalName.text != "")
        {
            Name = FinalName.text;
        }
        string[] files = FileUtils.GetFilesInPersistentFolder("VehicleDesign");
        VehicleSave vs = new();
        bool Exist = false;
        for (int i = 0; i < files.Length; i++)
        {
            string _name = Path.GetFileName(files[i]);
            _name = _name.Remove(_name.Length - 5);

            if (_name == Name)
            {
                Exist = true;
                break;
            }
        }

        if (!Exist)
        {
            List<string> New = new List<string>();

            New.Add(Name);

            New.Add((float.Parse(mv.RDR[Choosed][1]) - float.Parse(mv.RDR[Choosed][0]) + 1).ToString());

            for (int i = 4; i < mv.RDR[Choosed].Count; i++)
            {
                New.Add(mv.RDR[Choosed][i].ToString());
            }

            vs.carsave = new string[New.Count];
            for (int i = 0; i < New.Count; i++)
            {
                vs.carsave[i] = New[i];
            }

            var json = JsonUtility.ToJson(vs, true);

            var path = Path.Combine(Application.persistentDataPath, "VehicleDesign", Name + ".json");
            File.WriteAllText(path, json);

            NameInput.text = "设计1";
            mv.anim.SetTrigger("Back");
            mv.anim.SetTrigger("Cancel");
            print("SaveDesign");
            AssetDatabase.Refresh();
        }
        else
        {
            //动画是否替换当前设计
            ShowCarName.text = "是否替换同名设计";
            mv.anim.Play("ReplaceConfirm");
        }
        AssetDatabase.Refresh();
    }
#endif
}
