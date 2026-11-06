using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class YourSave
{
    public string[] car_save;
    public string[] block_save;

    public string SceneName;
    public int RightNum;
    public int DownNum;
    public string[] SceneList;
}


public class BlockManager : MonoBehaviour
{
    public static BlockManager bm;

    public Transform BlockList;
    public Transform StartButton;
    public GameObject InButton;
    public GameObject OutButton;
    public GameObject ReStartButton;
    public GameObject BlockContainer;
    public GameObject NextButton;
    public GameObject HomeButton;
    public GameObject TimeScaleButton;
    public GameObject PauseButton;
    public GameObject PausePanel;
    public GameObject BackButton;
    public GameObject MV;
    public CodeManager CM;
    public string animPath;
    public bool Rouge;
    public int LeftTimes;

    public List<List<(string kind, int f,List<Component>)>> ButtonLink = new();

    float AimScale = 1;
    private void Awake()
    {
        bm = this;
        Time.timeScale = 1f;
        Application.targetFrameRate = 120;

        var _path = Path.Combine(Application.persistentDataPath, "LevelSave");
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        LevelNext();
    }

    public List<string> save;//114514为载具终止符，负值代表依附于前一个方块中，-1000代表方向

    public List<float> ItemList; //正数为编号，后一项为数量

    public List<Vector2Int> KPB = new();
    public void KeepRotateBlock(int kind, int oc, int _index,int _inside)
    {
        KPB.Add(new Vector2Int(kind,oc));
        KPB.Add(new Vector2Int(_index,_inside));
    }

    void StartRotateBlock()
    {
        List<OwnCar> ocl = FindCar();
        for (int i = 0; i < KPB.Count; i += 2)
        {
            ocl[KPB[i].y].RotateTo(new(KPB[i + 1].x, KPB[i + 1].y), KPB[i].x);
        }
        KPB.Clear();
    }
   
    public void RefreshLevel(int ocl_index = -1,List<string> send_car_design = null)//加载存档
    {
        List<OwnCar> ocl = FindCar();
        List<string> car_design = new();
        if (ocl_index != -1)
        {
            //指定加载的对象
            var k = ocl[ocl_index];
            ocl.Clear();
            ocl.Add(k);
            //指定加载的内容
            car_design.AddRange(send_car_design);
        }
        else
        {
            //不是就加载save中的
            car_design.AddRange(save);
        }
        int count = 0;
        GameObject last_go = null;

        for (int i = 0; i < ocl.Count; i++)
        {
            ocl[i].ClearCar();
            int index = -1;
            int inside = 0;
            for (int a = 0; count < car_design.Count; a++)
            {
                if (car_design[count] == "114514")
                {
                    index = -1;
                    count++;
                    break;
                }
                if (car_design[count] == "0")
                {
                    count++;
                    index++;
                    inside = 0;
                    continue;
                }
                float BS = float.Parse(car_design[count]);
                if (Mathf.Abs(BS)<1000f)
                {
                    if (BS >= 0)//index格子的第几个
                    {
                        index++;
                        inside = 0;
                    }
                    else//内部的第几个
                    {
                        inside++;
                    }

                    float add = Mathf.Abs(BS);
                    if (add != 0)
                    {
                        try
                        {
                            string n = GameManager.BlockIndex[(int)add];
                            last_go = PutBlock(n, index,ocl[i].transform);
                            //对战模式下，要把物体设置为可碰撞
                            if (ocl_index != -1)
                            {
                                if (last_go.TryGetComponent(out Rigidbody2D rb))
                                {
                                    rb.isKinematic = true;
                                    rb.simulated = true;
                                }
                            }
                            if (Mathf.Abs(BS) == 46 && car_design[count + 1] == "StartCode")//IC
                            {

                                IC_Code IC = last_go.GetComponent<IC_Code>();
                                for (int num = 0; num < 100; num++)
                                {
                                    IC.OutCode[num] = car_design[count + num + 2];
                                }
                                IC.CreatList();
                                count += 101;
                            }
                        }
                        catch
                        {

                        }
                    }

                    
                }
                else
                {
                    if (BS <= -1000f)
                    {
                        KeepRotateBlock(-(int)BS % 1000, (ocl_index == -1)? i: ocl_index, index, inside);
                        if (OwnCar.GetT(last_go, out TryConnect tc)) tc.layIn = -(int)BS / 1000 - 1;//设置layer
                    }
                    else
                    {
                      
                    }
                }
                count++;
            }
        }
        StartRotateBlock();
        //block

        //当对战模式时不运行
        if (ocl_index == -1)
        {
            for (int i = 0; i < BlockList.transform.childCount; i++)
            {
                Destroy(BlockList.transform.GetChild(i).gameObject);
            }
            OrderContainer bm = BlockList.GetComponent<OrderContainer>();
            SwarmBlock sb;
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i] > 0)
                {
                    bm.Add();
                    GameObject bk = Resources.Load("GameObject/" + GameManager.BlockIndex[(int)ItemList[i]]) as GameObject;
                    sb = BlockList.GetChild(BlockList.childCount - 1).GetComponent<SwarmBlock>();
                    sb.BlockKind = bk;
                    sb.Num = (int)ItemList[i + 1];
                    sb.Refresh();
                    sb.SetHeath();
                    i++;
                }
            }
        }
    }
    GameObject PutBlock(string Kind,int to,Transform tf)
    {
        GameObject g = Resources.Load("GameObject/" + Kind) as GameObject;
        GameObject go = Instantiate(g);
        go.name = g.name;
        FollowMouse fm = go.AddComponent<FollowMouse>();
       // fm.AllHeath = h;
        fm.ClickTo(tf,to);
        return go;
    }
    public List<string> KeepLevel(int ocl_index = -1)//序列化
    {
        List< OwnCar> ocl = FindCar();
        if (ocl_index != -1)
        {
            //指定加载的对象
            var k = ocl[ocl_index];
            ocl.Clear();
            ocl.Add(k);
        }
        save .Clear();
        ItemList .Clear();
        for (int i = 0; i < ocl.Count; i++)
        {
            List<List<GameObject>> llg = ocl[i].AllObject;
            for (int ii = 0; ii < llg.Count; ii++)
            {
                if (llg[ii].Count == 0)
                {
                    save.Add("0");
                }
                else
                {
                    for (int ia = 0; ia < llg[ii].Count; ia++)
                    {
                        if (llg[ii][ia] == null)
                        {
                            save.Add("0");
                            break;
                        }

                        string n = llg[ii][ia].name;
                        if (n.IndexOf("(") != -1)
                        {
                            n = n.Remove(n.IndexOf("("));
                        }
                        int add = GameManager.BlockIndex.IndexOf(n);
                        if (add != -1)
                        {
                            if (ia > 0)
                            {
                                add *= -1;
                            }
                            save.Add(add.ToString());
                            if (Mathf.Abs(add) == 46)//ICcode追加
                            {
                                IC_Code IC = llg[ii][ia].GetComponent<IC_Code>();
                                save.Add("StartCode");
                                for (int num = 0; num < 100; num++)
                                {
                                    save.Add(IC.OutCode[num]);
                                }
                            }
                        }
                        else
                        {
                            if (ia > 0)
                            {
                            }
                            else
                            {
                                save.Add("0");
                                break;
                            }
                        }
                        if (OwnCar.GetT(llg[ii][ia], out TryConnect tc))
                        {
                            save.Add((-1000 * (tc.layIn + 1) - tc.forward - (tc.TryGetComponent(out IFlip Flip) && Flip.flip ? 10 : 0)).ToString());
                        }
                    }
                }
            }
            save.Add("114514");
        }
        //对战模式不运行
        if (ocl_index == -1)
        {
            for (int i = 0; i < BlockList.transform.childCount; i++)
            {
                SwarmBlock sb = BlockList.transform.GetChild(i).GetComponent<SwarmBlock>();
                if (sb.Num > 0)
                {
                    if (sb.BlockKind != null)
                    {
                        string nn = sb.BlockKind.name;
                        if (nn.IndexOf("(") != -1)
                        {
                            nn = nn.Remove(nn.IndexOf("("));
                        }
                        float n = GameManager.BlockIndex.IndexOf(nn);
                        ItemList.Add(n);
                        ItemList.Add(sb.Num);
                    }
                }
            }
        }
        return save;
    }
    public List<OwnCar> FindCar()
    {
        List<OwnCar> ol = new();
        ol.AddRange(FindObjectsOfType<OwnCar>());
        if (ol.Count > 0)
        {
            bool ok;
            int max = 0;
            do
            {
                max++;
                ok = true;
                for (int i = 0; i < ol.Count - 1; i++)//排序
                {
                    if (ol[i].transform.position.x > ol[i + 1].transform.position.x)
                    {
                        ol.Insert(i + 2, ol[i]);
                        ol.RemoveAt(i);
                        ok = false;
                    }
                    else
                    {
                        if (ol[i].transform.position.x == ol[i + 1].transform.position.x)
                        {
                            if (ol[i].transform.position.y > ol[i + 1].transform.position.y)
                            {
                                ol.Insert(i + 2, ol[i]);
                                ol.RemoveAt(i);
                                ok = false;
                            }
                        }
                    }
                }
            } while (!ok && max < 100);
        }
        return ol;
    }
    float time = 1f;

    float ST;
    public void X2(TimeScaleButton tsb)
    {
        if (Time.time - ST > 2f)
        {
            if (time == 1f)
            {
                time = 5f;
            }
            else
            {
                time = 1f;
            }
            AimScale = time;
            tsb.Set(time);
        }
    }


    public void SetRestart()
    {
        GameObject.Find("End").GetComponent<Animator>().Play("Flag");
        for (int i = 0; i < buttonManager.Count; i++)
        {
            Destroy(buttonManager[i].gameObject);
        }
        ButtonLink .Clear();
        buttonManager .Clear();

        Camera.main.GetComponent<LevelCamera>().TurnTo("Ready");
        _ = StartCoroutine(nameof(Late));
    }

    public void MV_Special()
    {
        RefreshLevel();
    }
    IEnumerator Late()
    {
        yield return null;
        yield return null;
        RefreshLevel();

    }
    IEnumerator Late2()
    {
        yield return null;
        yield return null;
        Camera.main.GetComponent<LevelCamera>().TurnTo("Start");
    }
    public void GameState(string state)
    {
        if (state == "Start")
        {
            KeepLevel();
            _ = StartCoroutine(nameof(Late2));
        }
        else
        {
            Camera.main.GetComponent<LevelCamera>().TurnTo(state);
        }
    }
    float LastScale = 1;
    float LastTimeScale = 1;
    public void Pause()
    {
        if (Time.timeScale == 0)
        {
            AimScale = LastScale;
            Time.timeScale = LastTimeScale;
            PausePanel.SetActive(false);
            BackButton.SetActive(false);
            Camera.main.GetComponent<AudioSource>().Play();
        }
        else
        {
            LastTimeScale = Time.timeScale;
            Time.timeScale = 0;
            LastScale = AimScale;
            AimScale = 0;
            PausePanel.SetActive(true);
            BackButton.SetActive(true);
            Camera.main.GetComponent<AudioSource>().Pause();
        }
    }
    public void Home()
    {
        AimScale = 1;
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
    public void Next()
    {
        if (!Rouge)
        {
            string ln = SceneManager.GetActiveScene().name;
            string nn = "Level" + ((int)float.Parse(ln.Replace("Level", "")) + 1);
            SceneManager.LoadScene(nn);
        }
        else
        {
            SaveMap();
            SceneManager.LoadScene("LevelChoose");
        }
    }
    public void GetBlock(GameObject kind ,int num)
    {
        if (!this.enabled) return;
        if (kind == null) return;
        if (kind.name.IndexOf("(") != -1)
        {
            kind.name = kind.name.Remove(kind.name.IndexOf("("));
        }
        for (int i = 0; i < BlockList.childCount; i++)
        {
            if (BlockList.GetChild(i).TryGetComponent(out SwarmBlock sb))
            {
                string st = sb.BlockKind.name;
                if (st.IndexOf("(") != -1)
                {
                    st = st.Remove(st.IndexOf("("));
                }
                if (kind.name == st)
                {
                    sb.Num += num;
                    sb.gameObject.SetActive(sb.Num > 0);
                    return;
                }
            }
        }
        SwarmBlock _sb = Instantiate(BlockContainer, BlockList).GetComponent<SwarmBlock>();
        _sb.Num = num;
        _sb.BlockKind = kind;
        _sb.Refresh();
    }
    public void GetBlock(string kind, Transform tf)
    {

        MV_Manager.GetRecord();

        if (BlockList == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("WTF"))
                {
                    BlockList = transform.GetChild(i);
                    break;
                }
            }
        }
        if (kind.IndexOf("(") != -1)
        {
            kind = kind.Remove(kind.IndexOf("("));
        }
        for (int i = 0; i < BlockList.childCount; i++)
        {
            if (BlockList.GetChild(i).TryGetComponent(out SwarmBlock sb))
            {
                string st = sb.BlockKind.name;
                if (st.IndexOf("(") != -1)
                {
                    st = st.Remove(st.IndexOf("("));
                }
                if (kind == st)
                {
                    sb.Num++;
                    sb.gameObject.SetActive(sb.Num > 0);
                    Destroy(tf.gameObject);
                    return;
                }
            }
        }
        SwarmBlock _sb = Instantiate(BlockContainer, BlockList).GetComponent<SwarmBlock>();
        _sb.Num = 1;
        tf.gameObject.name = kind;
        _sb.BlockKind = tf.gameObject;
        Vector3 ks = tf.lossyScale;
        tf.parent = _sb.transform;
        tf.localScale = ks;
        tf.gameObject.SetActive(false);
        _sb.Refresh();
    }
    public void LevelNext()
    {
        for (int i = 0; i < buttonManager.Count; i++)
        {
            Destroy(buttonManager[i].gameObject);
        }
        ButtonLink .Clear();
        buttonManager .Clear();

        if (!Rouge)
        {
            string SceneName = SceneManager.GetActiveScene().name;
            if (SceneName.Contains("Level"))
            {
                int _new = (int)float.Parse(SceneName.Replace("Level", ""));

                if (PlayerPrefs.GetInt("LevelUnLock", 3) < _new)
                {
                    PlayerPrefs.SetInt("LevelUnLock", _new);
                    PlayerPrefs.Save();
                }
            }
        }
    }
    void Update()
    {
        Application.targetFrameRate = 120;
        float tt = 15f - (1f / Time.deltaTime);
        if (tt <= 1)
        {
            if (AimScale < Time.timeScale)
            {
                Time.timeScale = AimScale;
            }
            else
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, AimScale, 0.0075f);
            }
        }
        else
        {
            if (AimScale < Time.timeScale)
            {
                Time.timeScale = AimScale;
            }
            else
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, Mathf.Lerp(AimScale, 1f, tt), 0.5f);
            }
        }

        if (BlockList == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("WTF"))
                {
                    BlockList = transform.GetChild(i);
                    break;
                }
            }
        }
        if (gs != GameManager.gameState)
        {
            gs = GameManager.gameState;
            switch (GameManager.gameState)
            {
                case GameManager.GameState.Watch:
                    BlockList.gameObject.SetActive(false);
                    StartButton.gameObject.SetActive(false);
                    InButton.SetActive(true);
                    OutButton.SetActive(false);
                    ReStartButton.SetActive(false);
                    TimeScaleButton.SetActive(false);
                    MV.SetActive(false);

                    break;
                case GameManager.GameState.Ready:
                    AimScale = 1;
                    BlockList.gameObject.SetActive(true);
                    HomeButton.SetActive(false);
                    MV.SetActive(true);
                    NextButton.SetActive(false);

                    StartButton.gameObject.SetActive(true);

                    TimeScaleButton.SetActive(false);
                    OutButton.SetActive(true);
                    InButton.SetActive(false);
                    ReStartButton.SetActive(false);

                    break;
                case GameManager.GameState.Start:
                    ST = Time.time;
                    SaveMapS();
                    BlockList.gameObject.SetActive(false);
                    StartButton.gameObject.SetActive(false);
                    OutButton.SetActive(false);
                    InButton.SetActive(false);
                    HomeButton.SetActive(false);
                    MV.SetActive(false);
                    TimeScaleButton.SetActive(true);
                    TimeScaleButton.GetComponent<TimeScaleButton>().Set(1);
                    if ((LeftTimes > 0 && Rouge) || !Rouge)
                    {
                        ReStartButton.SetActive(true);
                    }
                    break;
                case GameManager.GameState.Win:
                    BlockList.gameObject.SetActive(false);
                    StartButton.gameObject.SetActive(false);
                    OutButton.SetActive(false);
                    InButton.SetActive(false);
                    ReStartButton.SetActive(false);
                    TimeScaleButton.SetActive(false);
                    HomeButton.SetActive(true);
                    LevelNext();
                    NextButton.SetActive(true);
                    HomeButton.SetActive(false);
                    MV.SetActive(false);
                    if (Rouge)
                    {
                        BackLoss();
                        SaveMap();
                        print("BackLoss");
                    }
                    break;
            }
        }
    }
    GameManager.GameState gs = GameManager.GameState.Ready;
    public GameObject Button;
    public List<ButtonManager> buttonManager = new();

    
    private void Start()
    {
        if (BlockList == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("WTF"))
                {
                    BlockList = transform.GetChild(i);
                    break;
                }
            }
        }
        if (Rouge)
        {
            LoadMap();
            RefreshLevel();
            StartCoroutine(nameof(Late4));
        }
        else
        {
            animPath = "anim/" + PlayerPrefs.GetString("animPath", "BadPig");
            if (LoadMapS())
            {
                RefreshLevel();
                StartCoroutine(nameof(Late4));
            }
        }
    }
    IEnumerator Late4()
    {
        yield return null;
        yield return null;
        MV_Manager.GetRecord();
    }


    public void BackLoss()
    {
        List<int> waitDelete = new();
        List<int> BlockNull = new();
        List<OwnCar> ocl = FindCar();
        int count = 0;
        for (int i = 0; i < ocl.Count; i++)
        {

            int index = 0;

            int last = -1;

            List<List<GameObject>> gll = ocl[i].AllObject;
            for (count = 0; count < save.Count; count++)
            {
                if (save[count] == "114514")
                {
                    count++;
                    break;
                }
                if (save[count] == "0")
                {
                    index++;
                    continue;
                }
                if (Mathf.Abs(float.Parse(save[count])) < 100f)
                {
                    print(index + "," + count);
                    last = count;

                    print(gll[index].Count);
                    if (float.Parse(save[last]) < 0)
                    {
                        index--;
                    }

                    GameObject glo = gll[index][float.Parse(save[count]) > 0 ? 0 : 1];

                    index++;

                    if (glo == null)
                    {
                        if (float.Parse(save[last]) > 0)
                        {
                            BlockNull.Add(last);
                        }
                        waitDelete.Add(last);
                    }
                }
            }
        }
        print(BlockNull.Count);
        for (int i = waitDelete.Count - 1; i > -1; i--)
        {
            if (BlockNull.Contains(waitDelete[i]))
            {
                save[waitDelete[i]] = "0";
            }
            else
            {
                save[waitDelete[i]] = "4040";
            }
        }
        save.RemoveAll(s => s == "4040");
    }

    public void SaveMap()
    {
        print(GameManager.gameState);
        if (GameManager.gameState == GameManager.GameState.Win)
        {
            var path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
            string json = File.ReadAllText(path);
            var test = JsonUtility.FromJson<YourSave>(json);


            //change Item And Save
            test.car_save = new string[save.Count];
            for (int i = 0; i < save.Count; i++)
            {
                test.car_save[i] = save[i].ToString();
            }
            test.block_save = new string[ItemList.Count];
            for (int i = 0; i < ItemList.Count; i++)
            {
                test.block_save[i] = ItemList[i].ToString();
            }
            test.SceneName = "LevelChoose";
            test.SceneList[0] = test.RightNum.ToString();
            test.SceneList[1] = test.DownNum.ToString();
            test.SceneList[2] = "Pass";
            json = JsonUtility.ToJson(test, true);
            path = Path.Combine(Application.persistentDataPath, "LevelSave", "Rouge.json");
            Debug.Log("SaveNew");
            File.WriteAllText(path, json);
        }
    }

    public void SaveMapS()
    {
        var path = Path.Combine(Application.persistentDataPath, "LevelSave", SceneManager.GetActiveScene().name + ".json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var test = JsonUtility.FromJson<YourSave>(json);


            //change Item And Save
            test.car_save = new string[save.Count];
            for (int i = 0; i < save.Count; i++)
            {
                test.car_save[i] = save[i].ToString();
            }
            test.block_save = new string[ItemList.Count];
            for (int i = 0; i < ItemList.Count; i++)
            {
                test.block_save[i] = ItemList[i].ToString();
            }
            json = JsonUtility.ToJson(test, true);
            path = Path.Combine(Application.persistentDataPath, "LevelSave", SceneManager.GetActiveScene().name + ".json");
            File.WriteAllText(path, json);
        }
        else
        {
            YourSave test = new();

            //change Item And Save
            test.car_save = new string[save.Count];
            for (int i = 0; i < save.Count; i++)
            {
                test.car_save[i] = save[i].ToString();
            }
            test.block_save = new string[ItemList.Count];
            for (int i = 0; i < ItemList.Count; i++)
            {
                test.block_save[i] = ItemList[i].ToString();
            }
            var json = JsonUtility.ToJson(test, true);
            path = Path.Combine(Application.persistentDataPath, "LevelSave", SceneManager.GetActiveScene().name + ".json");
            Debug.Log("SaveNew");

            File.WriteAllText(path, json);
        }
    }

    public bool LoadMap()
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
            return false;
        }
        var test = JsonUtility.FromJson<YourSave>(json);
        animPath = "anim/"+test.SceneList[5];
        save .Clear();
        for (int i = 0; i < test.car_save.Length; i++)
        {
            save.Add(test.car_save[i]);
        }
        ItemList .Clear();
        for (int i = 0; i < test.block_save.Length; i++)
        {
            ItemList.Add(float.Parse(test.block_save[i]));
        }
        return true;
    }
    public bool LoadMapS()
    {
        var path = Path.Combine(Application.persistentDataPath,"LevelSave", SceneManager.GetActiveScene().name + ".json");
        string json;
        if (File.Exists(path))
        {
            json = File.ReadAllText(path);
        }
        else
        {
            Debug.Log("Can't find Map");
            return false;
        }
        var test = JsonUtility.FromJson<YourSave>(json);
        save .Clear();
        for (int i = 0; i < test.car_save.Length; i++)
        {
            save.Add(test.car_save[i]);
        }
        ItemList .Clear();
        for (int i = 0; i < test.block_save.Length; i++)
        {
            ItemList.Add(float.Parse(test.block_save[i]));
        }
        return true;
    }


    //下面这段是抄的，能跑
    private EventSystem eventSystem;
    private PointerEventData eventData;
    private bool init = false;
    void Init()
    {
        if (!init)
        {
            eventSystem = EventSystem.current;
            eventData = new PointerEventData(eventSystem);
            init = true;
        }
    }
    public bool IsOnUIElement(Vector2 pos)
    {
        Init();
        eventData.pressPosition = pos;
        eventData.position = pos;
        List<RaycastResult> list = new();
        EventSystem.current.RaycastAll(eventData, list);


        foreach (var temp in list)
        {
            if (temp.gameObject.layer.Equals(5))
            {
                return true;//Equal(5)中的“5”是指图层第五层UI层
            }
        }
        return false;
    }
    //抄的到这里结束
}
