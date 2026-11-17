using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 指令代码百科：一个指令一「本」，本里按「方面」分页；左右箭头在同一本里翻页，换指令靠点代码块。
/// 布局由预制体排好，这里只负责填字、找图、算线、量字号，不动任何 RectTransform。
/// </summary>
public class CodeInform : MonoBehaviour
{
    public Text text;//标题
    public Animator anim;//Canvas 上那个 Animator（MoveVehicle）

    [Header("四块说明（留空按名字找）")]
    public Text upText;
    public Text downText;
    public Text leftText;
    public Text rightText;

    [Header("图标（留空按名字找）")]
    public Image icon;

    [Header("翻页（留空按名字找）")]
    public Button prevButton;
    public Button nextButton;
    public Text prevPageText;
    public Text nextPageText;

    [Header("引线")]
    public float lineWidth = 2f;
    public Color lineColor = Color.white;
    [Tooltip("线末端落在图标边框内侧的比例，1 表示正好贴边")]
    public float endInset = 0.86f;
    public Color varTextColor = new(0.16f, 0.16f, 0.16f, 1f);

    private Text varText;//图标上的可变字：变量名、按钮名、跳转编号这类玩家填的部分
    private readonly List<Image> lines = new();
    private Sprite lineSprite;
    private int codeIndex = -1;//当前指令在代码表里的下标
    private int page;//当前指令里的第几页
    private float titleDefaultSize = 200f;
    private float upDefaultSize = 175f;
    private float downDefaultSize = 175f;
    private float leftDefaultSize = 175f;
    private float rightDefaultSize = 175f;
    private readonly List<GameManager.CodePage> pages = new();

    // kind -> 图标贴图 / 颜色，从 CodeEdit 里现成的指令块上抄
    private readonly Dictionary<string, Sprite> spriteOf = new();
    private readonly Dictionary<string, Color> colorOf = new();

    private static readonly string[] Dirs = { "上", "下", "左", "右" };
    private static readonly Vector2[] DirVecs = { new(0f, 1f), new(0f, -1f), new(-1f, 0f), new(1f, 0f) };

    private static List<GameManager.CodeInfo> Codes => GameManager.CodeLibrary;

    #region 建立
    private void Awake()
    {
        if (text != null && text.fontSize > 0) titleDefaultSize = text.fontSize;
        if (upText != null && upText.fontSize > 0) upDefaultSize = upText.fontSize;
        if (downText != null && downText.fontSize > 0) downDefaultSize = downText.fontSize;
        if (leftText != null && leftText.fontSize > 0) leftDefaultSize = leftText.fontSize;
        if (rightText != null && rightText.fontSize > 0) rightDefaultSize = rightText.fontSize;

        BuildLineSprite();
        BuildLinePool();
        BuildVarText();
        BuildIconMap();
        HookButtons();
    }

    private void BuildLineSprite()
    {
        Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        lineSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        lineSprite.hideFlags = HideFlags.HideAndDontSave;
    }

    private void BuildLinePool()//四个方向各一条，够用
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new("线", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();//压在最下面，别盖住字和图标
            Image im = go.GetComponent<Image>();
            im.sprite = lineSprite;
            im.color = lineColor;
            im.raycastTarget = false;
            go.SetActive(false);
            lines.Add(im);
        }
    }

    private void BuildVarText()//图标上的可变字
    {
        GameObject go = new("可变字", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(transform, false);
        varText = go.GetComponent<Text>();
        varText.font = text != null ? text.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        varText.fontSize = 30;
        varText.alignment = TextAnchor.MiddleCenter;
        varText.color = varTextColor;
        varText.horizontalOverflow = HorizontalWrapMode.Wrap;
        varText.verticalOverflow = VerticalWrapMode.Truncate;
        varText.resizeTextForBestFit = true;
        varText.resizeTextMinSize = 10;
        varText.resizeTextMaxSize = 32;
        varText.raycastTarget = false;
        varText.supportRichText = false;
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(78f, 54f);
        rt.anchoredPosition = Vector2.zero;
        go.SetActive(false);
    }

    private void BuildIconMap()
    {
        Transform edit = transform.parent != null ? transform.parent.parent : null;//代码百科 -> CodeEdit
        if (edit == null) return;
        foreach (SwarmCode sc in edit.GetComponentsInChildren<SwarmCode>(true))
        {
            if (sc == null || string.IsNullOrEmpty(sc.kind)) continue;
            Image im = sc.GetComponent<Image>();
            if (im == null || im.sprite == null) continue;
            spriteOf[sc.kind] = im.sprite;
            colorOf[sc.kind] = im.color;
        }
    }

    private void HookButtons()
    {
        if (prevButton != null)
        {
            prevButton.onClick.RemoveListener(Prev);
            prevButton.onClick.AddListener(Prev);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(Next);
            nextButton.onClick.AddListener(Next);
        }
    }
    #endregion

    #region 对外
    public void Show(string kind)//点哪个代码块就看哪个指令；百科没打开就不动
    {
        if (anim != null && !anim.GetBool("百科")) return;
        int hit = FindCode(kind);
        if (hit < 0) return;
        codeIndex = hit;
        page = 0;
        LoadPages();
        Render(kind);
    }

    public void Hide()//收起代码百科
    {
        anim.SetBool("百科", false);
    }

    public void Toggle()//百科按钮：开着就收，关着就再展开
    {
        anim.SetBool("百科", !anim.GetBool("百科"));
    }

    public void Prev()//翻到这本里的上一页
    {
        if (codeIndex < 0 || page <= 0) return;
        page--;
        Render(CurrentKind);
    }

    public void Next()//翻到这本里的下一页
    {
        if (codeIndex < 0 || page >= pages.Count - 1) return;
        page++;
        Render(CurrentKind);
    }
    #endregion

    #region 数据
    private string CurrentKind
    {
        get
        {
            if (codeIndex < 0 || codeIndex >= Codes.Count) return "";
            return Codes[codeIndex].kind;
        }
    }

    private int FindCode(string kind)
    {
        if (string.IsNullOrEmpty(kind)) return codeIndex;
        for (int i = 0; i < Codes.Count; i++)
        {
            if (Codes[i].kind == kind) return i;
        }
        for (int i = 0; i < Codes.Count; i++)//带名字的指令退回前缀那一条
        {
            if (kind.StartsWith(Codes[i].kind)) return i;
        }
        return -1;
    }

    /// <summary>取出这个指令的页表；旧数据没有 pages 的，按 notes 兜一页出来。</summary>
    private void LoadPages()
    {
        pages.Clear();
        if (codeIndex < 0 || codeIndex >= Codes.Count) return;
        GameManager.CodeInfo info = Codes[codeIndex];
        if (info.pages != null && info.pages.Count > 0)
        {
            pages.AddRange(info.pages);
            return;
        }
        if (info.notes != null && info.notes.Count > 0)
        {
            GameManager.CodePage p = new() { title = info.title };
            p.notes.AddRange(info.notes);
            pages.Add(p);
        }
    }

    private static int SlotOf(string to)//摆位：看 to 里第一个出现的方向
    {
        if (string.IsNullOrEmpty(to)) return 0;
        for (int i = 0; i < to.Length; i++)
        {
            switch (to[i])
            {
                case '上': return 0;
                case '下': return 1;
                case '左': return 2;
                case '右': return 3;
            }
        }
        return 0;
    }

    private Text SlotText(int slot)
    {
        return slot switch
        {
            0 => upText,
            1 => downText,
            2 => leftText,
            _ => rightText
        };
    }
    #endregion

    #region 渲染
    private void Render(string kind)
    {
        if (pages.Count == 0) return;
        if (page < 0) page = 0;
        if (page >= pages.Count) page = pages.Count - 1;

        GameManager.CodePage p = pages[page];
        if (text != null)
        {
            string titleText = p.title ?? "";
            text.text = ApplyRichTags(titleText, titleDefaultSize);
            text.gameObject.SetActive(!string.IsNullOrEmpty(titleText));
        }

        string[] slotText = new string[4];
        List<string>[] slotLines = { new(), new(), new(), new() };
        AssignSlots(p, slotText, slotLines);

        SetSlot(upText, slotText[0], upDefaultSize);
        SetSlot(downText, slotText[1], downDefaultSize);
        SetSlot(leftText, slotText[2], leftDefaultSize);
        SetSlot(rightText, slotText[3], rightDefaultSize);

        string baseKind = BaseKind(kind);
        if (icon != null)
        {
            if (spriteOf.TryGetValue(baseKind, out Sprite sp))
            {
                icon.sprite = sp;
                icon.color = colorOf[baseKind];
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }
        if (varText != null)
        {
            string remain = VarTextOf(kind, baseKind);
            varText.text = remain;
            varText.gameObject.SetActive(icon != null && icon.enabled && !string.IsNullOrEmpty(remain));
            if (icon != null) varText.rectTransform.anchoredPosition = icon.rectTransform.anchoredPosition + new Vector2(0f, 9f);
        }

        Canvas.ForceUpdateCanvases();
        FixSemanticLineBreaks(text);
        FixSemanticLineBreaks(upText);
        FixSemanticLineBreaks(downText);
        FixSemanticLineBreaks(leftText);
        FixSemanticLineBreaks(rightText);

        DrawLines(slotLines);
        UpdatePageLabel();
    }

    private static void AssignSlots(GameManager.CodePage p, string[] slotText, List<string>[] slotLines)
    {
        float[] used = new float[4];
        if (p == null || p.notes == null) return;
        for (int i = 0; i < p.notes.Count; i++)
        {
            GameManager.CodeNote n = p.notes[i];
            if (n == null || string.IsNullOrEmpty(n.text)) continue;
            string pos = string.IsNullOrEmpty(n.to) ? n.line : n.to;
            int slot = SlotOf(pos);
            float u = TextUnits(n.text);
            if (slot >= 2 && u > 12f)
                slot = (!string.IsNullOrEmpty(pos) && pos.Contains('下')) ? 1 : (used[0] <= used[1] ? 0 : 1);
            float per = slot < 2 ? 20f : 5f;
            used[slot] += Mathf.Max(1f, Mathf.Ceil(u / per));
            slotText[slot] = string.IsNullOrEmpty(slotText[slot]) ? n.text : slotText[slot] + (char)10 + n.text;
            if (!string.IsNullOrEmpty(n.line) && n.line != "none")
                slotLines[slot].Add(n.line);
        }
    }

    private static float TextUnits(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0f;
        float w = 0f;
        bool inTag = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '<') { inTag = true; continue; }
            if (c == '>') { inTag = false; continue; }
            if (inTag) continue;
            if (c == '\n') { w += 1000f; continue; }
            w += c > 127 ? 1f : 0.5f;
        }
        return w;
    }

    private static string ApplyRichTags(string content, float baseSize)
    {
        if (string.IsNullOrEmpty(content)) return "";
        int titleSz = Mathf.RoundToInt(baseSize * 1.15f);
        int smallSz = Mathf.RoundToInt(baseSize * 0.85f);
        int contentSz = Mathf.RoundToInt(baseSize);
        string res = content
            .Replace("<Title>", $"<size={titleSz}>")
            .Replace("</Title>", "</size>")
            .Replace("<Small>", $"<size={smallSz}>")
            .Replace("</Small>", "</size>")
            .Replace("<Content>", $"<size={contentSz}>")
            .Replace("</Content>", "</size>");
        // <nobr>...</nobr> 内部空白换成不换行空格，防止语义词被拆行
        int nbStart = res.IndexOf("<nobr>");
        while (nbStart >= 0)
        {
            int nbEnd = res.IndexOf("</nobr>", nbStart + 6);
            if (nbEnd < 0) break;
            string inner = res.Substring(nbStart + 6, nbEnd - (nbStart + 6)).Replace(' ', '\u00A0');
            res = res.Substring(0, nbStart) + inner + res.Substring(nbEnd + 7);
            nbStart = res.IndexOf("<nobr>");
        }
        return res.Replace("<nobr>", "").Replace("</nobr>", "");
    }

    private void SetSlot(Text t, string content, float defaultSize)
    {
        if (t == null) return;
        bool show = !string.IsNullOrEmpty(content);
        string processed = ApplyRichTags(content, defaultSize);
        if (t.text != processed) t.text = processed;
        if (t.gameObject.activeSelf != show) t.gameObject.SetActive(show);
    }

    /// <summary>检查 Unity 自动折行是否把 [真] / [假] / (提示) / 专有名词 从中间劈开，或把紧跟在后面的标点/括号单独挤到行首；若发生则整体推到下一行开头。</summary>
    private static void FixSemanticLineBreaks(Text t)
    {
        if (t == null || !t.gameObject.activeInHierarchy || string.IsNullOrEmpty(t.text)) return;
        for (int pass = 0; pass < 6; pass++)
        {
            Canvas.ForceUpdateCanvases();
            var linesInfo = t.cachedTextGenerator.lines;
            if (linesInfo == null || linesInfo.Count <= 1) return;
            string s = t.text;
            int insertPos = -1;
            for (int l = 1; l < linesInfo.Count; l++)
            {
                int startIdx = linesInfo[l].startCharIdx;
                if (startIdx <= 0 || startIdx >= s.Length) continue;
                int breakAt = FindBrokenTokenStart(s, startIdx);
                if (breakAt >= 0 && breakAt < startIdx)
                {
                    if (breakAt > 0 && s[breakAt - 1] != '\n')
                    {
                        insertPos = breakAt;
                        break;
                    }
                }
            }
            if (insertPos < 0) break;
            t.text = s.Insert(insertPos, "\n");
        }
    }

    private static int FindBrokenTokenStart(string s, int startIdx)
    {
        char c = s[startIdx];
        // 1) 行首落在闭合括号或中文孤立标点上，说明前面的 [xxx] / 标点前词被拆断了，往前退回一格再查
        if (c == ']' || c == '）' || c == ')' || c == '】' ||
            c == '，' || c == '。' || c == '；' || c == '、' || c == '：' || c == ',' || c == '.')
        {
            // 若行首是孤立标点且前面紧挨着富文本闭合标签（如 </b></color>），退到标签内文字或整个专有名词之前
            int back = startIdx - 1;
            while (back >= 0 && (s[back] == '>' || s[back] == ' '))
            {
                if (s[back] == '>')
                {
                    int openTag = s.LastIndexOf('<', back);
                    if (openTag < 0) break;
                    back = openTag - 1;
                }
                else
                {
                    back--;
                }
            }
            if (back >= 0)
            {
                int token = FindEnclosingOrColorStart(s, back);
                if (token >= 0) return token;
                // 至少把标点前的那个字带上一起换到下一行，避免标点放行首
                return back;
            }
            return -1;
        }

        if (c == '[' || c == '（' || c == '(' || c == '【') return -1;

        return FindEnclosingOrColorStart(s, startIdx);
    }

    private static int FindEnclosingOrColorStart(string s, int pos)
    {
        int scan = pos - 1;
        int dist = 0;
        bool inTag = false;
        while (scan >= 0 && dist < 24)
        {
            char ch = s[scan];
            if (ch == '\n') break;
            if (ch == '>') { inTag = true; scan--; continue; }
            if (ch == '<')
            {
                inTag = false;
                // 若遇到 <color=... 开头的标签，且我们正在它内部（没有先遇到 </color>），说明一个带颜色的专有名词被劈开
                if (s.IndexOf("<color", scan, System.StringComparison.Ordinal) == scan)
                    return scan;
                scan--;
                continue;
            }
            if (inTag) { scan--; continue; }

            // 已经穿过完整闭合符号或 </color>，说明不在它里面
            if (ch == ']' || ch == '）' || ch == ')' || ch == '】') break;
            if (ch == '[' || ch == '（' || ch == '(' || ch == '【')
            {
                int tagStart = scan;
                if (tagStart >= 1 && s[tagStart - 1] == '>')
                {
                    // 跳过紧挨在前面的 <b> 或 <color=...>
                    int cur = tagStart - 1;
                    while (cur >= 0 && s[cur] == '>')
                    {
                        int openTag = s.LastIndexOf('<', cur);
                        if (openTag < 0) break;
                        string tag = s.Substring(openTag, cur - openTag + 1);
                        if (tag.StartsWith("<color") || tag == "<b>")
                            tagStart = openTag;
                        else
                            break;
                        cur = openTag - 1;
                    }
                }
                return tagStart;
            }
            scan--;
            dist++;
        }
        return -1;
    }

    private void UpdatePageLabel()
    {
        int total = pages.Count;
        string label = total <= 0 ? "" : (page + 1) + "/" + total;
        if (prevPageText != null) prevPageText.text = label;
        if (nextPageText != null) nextPageText.text = label;
        if (prevButton != null) prevButton.gameObject.SetActive(total > 1 && page > 0);
        if (nextButton != null) nextButton.gameObject.SetActive(total > 1 && page < total - 1);
    }

    private string BaseKind(string kind)//"Num:速度" -> "Num:"
    {
        if (string.IsNullOrEmpty(kind)) return "";
        if (spriteOf.ContainsKey(kind)) return kind;
        for (int i = 0; i < Codes.Count; i++)
        {
            if (kind == Codes[i].kind) return Codes[i].kind;
        }
        for (int i = 0; i < Codes.Count; i++)
        {
            if (kind.StartsWith(Codes[i].kind)) return Codes[i].kind;
        }
        return kind;
    }

    private string VarTextOf(string kind, string baseKind)//玩家填的那一段：变量名、按钮名、跳转编号
    {
        if (string.IsNullOrEmpty(kind) || string.IsNullOrEmpty(baseKind)) return "";
        if (kind.Length <= baseKind.Length) return "";
        string rest = kind.Substring(baseKind.Length);
        if (rest == "{}" || rest == "}") return "";
        return rest;
    }
    #endregion

    #region 引线
    private void DrawLines(List<string>[] slotLines)
    {
        for (int i = 0; i < lines.Count; i++) lines[i].gameObject.SetActive(false);
        if (icon == null || !icon.enabled || slotLines == null) return;

        int used = 0;
        for (int d = 0; d < Dirs.Length; d++)
        {
            Vector2 end = DirEndLocal(d);
            Vector2 endWorld = icon.rectTransform.anchoredPosition + end;

            // 同一个方向终点只画一条：从最近的活跃说明框出发（相同终点自动合并）
            Text best = null;
            float bestDist = float.MaxValue;
            for (int s = 0; s < 4; s++)
            {
                if (slotLines[s] == null || slotLines[s].Count == 0) continue;
                bool hit = false;
                for (int k = 0; k < slotLines[s].Count; k++)
                {
                    string ln = slotLines[s][k];
                    if (string.IsNullOrEmpty(ln) || !ln.Contains(Dirs[d])) continue;
                    if (d == 1 && ln.Contains('上') && ln.Contains('下')) continue;
                    if (d == 3 && ln.Contains('左') && ln.Contains('右')) continue;
                    hit = true;
                    break;
                }
                if (!hit) continue;
                Text t = SlotText(s);
                if (t == null || !t.gameObject.activeSelf) continue;
                float dist = (t.rectTransform.anchoredPosition - endWorld).sqrMagnitude;
                if (dist < bestDist) { bestDist = dist; best = t; }
            }
            if (best == null) continue;

            Vector2 start = ClosestOnText(best, endWorld);
            if (used >= lines.Count) break;
            PlaceLine(lines[used], start, endWorld);
            used++;
        }
    }

    private Vector2 DirEndLocal(int d)
    {
        float half = icon.rectTransform.rect.width * 0.5f * endInset;
        return DirVecs[d] * half;
    }

    /// <summary>直接用 Text 生成的顶点包围盒算文字真实边界，线从边外侧 4 像素出发，不穿字也不悬空。</summary>
    private Vector2 ClosestOnText(Text t, Vector2 p)
    {
        if (t == null) return p;
        RectTransform rt = t.rectTransform;
        Transform parentTf = transform;

        var verts = t.cachedTextGenerator.verts;
        Rect textRect;
        if (verts != null && verts.Count >= 4)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < verts.Count; i++)
            {
                Vector3 localPos = verts[i].position / t.pixelsPerUnit;
                Vector3 worldPos = rt.TransformPoint(localPos);
                Vector3 inParent = parentTf.InverseTransformPoint(worldPos);
                if (inParent.x < minX) minX = inParent.x;
                if (inParent.x > maxX) maxX = inParent.x;
                if (inParent.y < minY) minY = inParent.y;
                if (inParent.y > maxY) maxY = inParent.y;
            }
            textRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
        else
        {
            Rect r = rt.rect;
            float sx = Mathf.Max(0.001f, Mathf.Abs(rt.localScale.x));
            float sy = Mathf.Max(0.001f, Mathf.Abs(rt.localScale.y));
            Vector2 c = rt.anchoredPosition;
            textRect = new Rect(c.x - r.width * sx * 0.5f, c.y - r.height * sy * 0.5f, r.width * sx, r.height * sy);
        }

        float x = Mathf.Clamp(p.x, textRect.xMin, textRect.xMax);
        float y = Mathf.Clamp(p.y, textRect.yMin, textRect.yMax);
        if (x == p.x && y == p.y) return textRect.center;
        Vector2 hit = new(x, y);
        Vector2 dir = p - hit;
        if (dir.sqrMagnitude > 0.01f) hit += dir.normalized * 4f;
        return hit;
    }

    private void PlaceLine(Image line, Vector2 start, Vector2 end)
    {
        Vector2 delta = end - start;
        float len = delta.magnitude;
        if (len < 0.5f)
        {
            line.gameObject.SetActive(false);
            return;
        }
        RectTransform rt = line.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (start + end) * 0.5f;
        rt.sizeDelta = new Vector2(len, lineWidth);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        line.color = lineColor;
        line.gameObject.SetActive(true);
    }
    #endregion

    private void OnEnable()
    {
        if (codeIndex < 0) codeIndex = 0;
        if (Codes.Count > 0)
        {
            if (codeIndex >= Codes.Count) codeIndex = Codes.Count - 1;
            if (page < 0) page = 0;
            LoadPages();
            Render(CurrentKind);
        }
    }
}
