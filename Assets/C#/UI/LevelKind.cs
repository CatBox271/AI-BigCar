using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelKind : MonoBehaviour ,IPointerDownHandler,IPointerMoveHandler
{
    public LevelManager LM;
    public string SceneName;
    public Vector3 SetPos;

    public GameObject Line;
    public List<GameObject> LineList;
    public List<Transform> LinkItem;

    public Text text;

    public MapInform Panel;

    bool Choose = false;public Image im;
    public void BeSelect()
    {
        RougeChoose RC = transform.parent.parent.GetComponent<RougeChoose>();
        int right = LM.LLK.FindIndex(s => s.Contains(this));
        int down = LM.LLK[right].FindIndex(s => s == this);
        RC.YS.DownNum = down;
        RC.YS.RightNum = right;
        RC.SaveMap();
        SceneManager.LoadScene(SceneName);
    }
    public void First()
    {
        if(Down)
        ShowInform();
    }

    public void ShowInform()
    {
        Panel.gameObject.SetActive(true);
        Panel.lk = this;
    }
    public void Choosed()
    {
        Choose = true;
    }
    bool _BeStart = false;
    public void BeStart()
    {
        _BeStart = true;
    }
    public void SetLine(List<Transform> LinkTo)
    {
        LinkItem = LinkTo;
        _ = StartCoroutine(nameof(late));
    }

    IEnumerator late()
    {
        yield return null;
        if (_BeStart)
        {
            GetComponent<Button>().interactable = true;
        }
        for (int i = 0; i < LinkItem.Count; i++)
        {
            GameObject go = Instantiate(Line, transform);
            go.transform.position = (transform.position + LinkItem[i].position) / 2f;
            Vector2 v2 = LinkItem[i].position - transform.position;
            go.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan(v2.y / v2.x) * 180f / 3.14f);
            if (Choose)
            {
                LinkItem[i].GetComponent<Button>().interactable = true;
            }
        }
    }
    Color origin;

    private void Start()
    {
        origin = im.color;
    }
    void Update()
    {
        transform.localPosition = SetPos + new Vector3(LM.PosAdd, 0);
        if (Panel.lk == this)
        {
            im.color = origin * 1;
        }
        else
        {
            im.color = origin * 0.75f;
        }
    }

    bool Down;
    public void OnPointerDown(PointerEventData eventData)
    {
        Down = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Down = false;
    }
}
