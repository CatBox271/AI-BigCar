using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintSystem : MonoBehaviour,IStart
{
    public GameObject SingleText;
    public int maxCount = 10;
    float updateTime = 0;
    LinkedList<Text> AllText = new();

    public IC_Code IC;
    private void Start()
    {
        updateTime = Time.time;
        switch (Random.Range(0, 120))
        {
            default:
                printStr("[纸箱]控制系统");
                printStr("------准备完毕------");
                IC.CodeWarning();
                break;
            case 0:
                printStr("[衹葙]啌淛係統");
                printStr("______痽備綄哔______");
                IC.CodeWarning();
                break;
            case 1:
                IC.CodeWarning();
                StartCoroutine(nameof(WantYouGone));
                break;
        }
    }

    public void printStr(string s = null)
    {
        GetFirst();
        AllText.First.Value.text = s;
        AllMove();

        StrTableAdd(s);
    }
    public readonly List<string> strTable = new();
    public readonly List<int> CountTable = new();
    public void PrintInfo(float f)
    {
        updateTime = Time.time + 5f;

        string s = f.ToString();
        if (float.IsInfinity(f))
        {
            s = float.IsPositiveInfinity(f).ToString();
        }
        int i = strTable.IndexOf(s);
        if (i != -1)
        {
            CountTable[i]++;
            GetText(i).text = s + " ×" + CountTable[i];
        }
        else
        {
            PrintCount++;
            GetFirst();
            AllText.First.Value.text = s;
            AllMove();

            StrTableAdd(s);
        }
    }

    int PrintCount;
    void StrTableAdd(string s)
    {
        strTable.Insert(0, s);
        CountTable.Insert(0, 1);

        if (strTable.Count > 5)
        {
            strTable.RemoveAt(5);
            CountTable.RemoveAt(5);
        }
    }
    Text GetText(int t)
    {
        var Node = AllText.First;
        for (int i = 0; i < t; i++)
        {
            Node = Node.Next;
        }
        return Node.Value;
    }
    void GetFirst()
    {
        if (AllText.Count < maxCount)//增加新的
        {
            GameObject go = Instantiate(SingleText, SingleText.transform.parent, true);
            go.SetActive(true);
            Text t = go.GetComponent<Text>();
            AllText.AddFirst(t);
            nodes.Add(t);
            move.Add(new());
        }
        else
        {
            var lastNode = AllText.Last;
            AllText.Remove(lastNode);
            AllText.AddFirst(lastNode);
        }
    }
    float CountCheck;
    bool Quick = false;
    public float f1 = 5;
    private void Update()
    {
        if (!StartRun) return;
        if (CountCheck < Time.time)
        {
            CountCheck = Time.time + 0.25f;
            Quick = PrintCount > f1;
            PrintCount = 0;
        }
        if (updateTime < Time.time)
        {
            updateTime += 5;
            printStr("...");
        }
    }
    List<Text> nodes = new();
    List<List<Coroutine>> move = new();

    bool StartRun;

    void AllMove()
    {
        var Node = AllText.First;
        for (int i = 0; i < AllText.Count; i++)//遍历更改位置
        {
            Transform tf = Node.Value.transform;
            float s = 0.01f;
            int d = nodes.IndexOf(Node.Value);
            if (Quick)
            {
                tf.localScale = Vector3.one * (i >= 5 ? 0 : 0.01f);
                tf.localPosition = new(0, -0.4f + 0.2f * i);
                if (i > 5) return;
                move[d].ForEach(s => StopCoroutine(s));
                move[d].Clear();

            }
            else
            {
                bool m = true;
                int add = 0;
                if (i == 0)
                {
                    tf.localPosition = new(0, -0.4f);
                    tf.localScale = new();
                    m = false;
                    add++;
                    move[d].ForEach(s => StopCoroutine(s));
                    move[d].Clear();
                }
                if (i == 5)
                {
                    s = 0;
                    m = false;
                    move[d].ForEach(s => StopCoroutine(s));
                    move[d].Clear();
                }
                if (i > 5) return;
                //if (move[d] != null) StopCoroutine(move[d]);
                move[d].Add(StartCoroutine("Move", (tf, m, add * -0.2f, s)));
            }
            Node = Node.Next;
        }
    }
    IEnumerator Move((Transform, bool, float, float) ti)
    {
        Transform tf = ti.Item1;
        bool move = ti.Item2;
        float t = ti.Item3;
        float s = ti.Item4;
        do
        {
            yield return null;
            t += Time.deltaTime * 1f;
            if (t < 0 || t > 0.2f) continue;
            tf.localScale = Mathf.Lerp(tf.localScale.x, s, t * 5f) * Vector3.one;
            if (move) tf.localPosition += new Vector3(0, t * Time.deltaTime * 10f);
        } while (true);
    }
    #region WantYouGone

    readonly List<string> wantYouGoneLyrics = new()
    {
    "Forms FORM-29827281-12-2:",
    "Notice of Dismissal",
    "",
    "Well here we are again",
    "It's always such a pleasure",
    "Remember when you tried",
        "to kill me twice?",
    "Oh how we laughed and laughed",
    "Except I wasn't laughing",
    "Under the circumstances",
    "I've been shockingly nice",
    "You want your freedom?",
        " Take it",
    "That's what I'm counting on",
    "I used to want you dead" ,
        " but",
    "Now I only want you gone",
    "She was a lot like you",
    "(Maybe not quite as heavy)",
    "Now little Caroline is in here too",
    "One day they woke me up",
    "So I could live forever",
    "It's such a shame the same" ,
        "will never happen to you",
    "You've got your" ,
        "short sad" ,
        "life left",
    "That's what I'm counting on",
    "I'll let you get right to it",
    "Now I only want you gone",
    "Goodbye my only friend",
    "Oh, did you think I meant you?",
    "That would be funny" ,
        "if it weren't so sad",
    "Well you have been replaced",
    "I don't need anyone now",
    "When I delete you maybe",
    "[REDACTED]",
    "Go make some new disaster",
    "That's what I'm counting on",
    "You're someone else's problem",
    "Now I only want you gone",
    "Now I only want you gone",
    "Now I only want you " ,
    "gone",
};
    readonly List<float> wantYouGoneLyricsWait = new()
{
    0,0,5.650f, 2.499f, 2.542f, 1.837f, 2.789f, 2.393f, 2.441f, 2.233f, 3.733f,
    2.417f, 2.017f, 5.075f, 3.149f, 0.792f, 7.165f, 2.592f, 2.558f, 4.660f, 2.652f,
    2.150f, 1.935f, 3.740f, 1.317f, 0.950f, 2.543f, 4.846f, 3.475f, 8.114f, 2.475f,
    2.084f, 1.975f, 3.075f, 2.434f, 2.105f, 2.859f, 3.116f, 4.358f, 5.234f, 3.425f,
    4.275f, 5.103f,0f,0f
};

    IEnumerator WantYouGone()
    {
        for (int i = 0; i < wantYouGoneLyrics.Count; i++)
        {
            printStr(wantYouGoneLyrics[i]);
            updateTime = Time.time;
            yield return new WaitForSeconds(wantYouGoneLyricsWait[i]);
        }
    }


    #endregion

    public void SetStart(float t)
    {
        StartRun = true;
        printStr();
        printStr();
        printStr();
        printStr();
        printStr();
    }
}
