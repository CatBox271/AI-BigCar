using UnityEditor;
using UnityEngine;

/// <summary>
/// CardIndexAttribute 的绘制器：把每个 int 画成卡片下拉框（选项来自 CardManager.CardDefine），
/// 行尾带「复制 / 删除」，添加用列表自带的 + 按钮。
/// 增删后立刻 ApplyModifiedProperties 再 ExitGUI，中止这一帧剩下的列表绘制，免得拿旧下标继续画。
/// 放在 Assets/C#/Editor 下，随 Assembly-CSharp-Editor 编译，不会进包体。
/// </summary>
[CustomPropertyDrawer(typeof(CardIndexAttribute))]
public class CardIndexDrawer : PropertyDrawer
{
    const float Gap = 2f;
    const float BtnW = 40f;

    /// <summary>按 CardManager.CardDefine 生成下拉文本；每次重画都重建，卡片表改了立刻跟上。</summary>
    static string[] BuildOptions()
    {
        var cards = CardManager.CardDefine;
        string[] options = new string[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            CardInfo c = cards[i];
            string name = string.IsNullOrEmpty(c.card_name) ? GameManager.GetBlockChineseName(c.block_kind) : c.card_name;
            string code = GameManager.GetBlockCodeName(c.block_kind);
            options[i] = i + "  " + name + (string.IsNullOrEmpty(code) ? "  （技能牌）" : " (" + code + ")");
        }
        return options;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] options = BuildOptions();

        Rect popupRect = new Rect(position.x, position.y, position.width - (BtnW + Gap) * 2, position.height);
        // 越界值（卡片表缩了）不静默改写，显示为空等用户重选
        int cur = property.intValue;
        if (cur < 0 || cur >= options.Length) cur = -1;
        property.intValue = EditorGUI.Popup(popupRect, cur, options);

        Rect copyRect = new Rect(popupRect.xMax + Gap, position.y, BtnW, position.height);
        if (GUI.Button(copyRect, "复制")) Copy(property);
        Rect removeRect = new Rect(copyRect.xMax + Gap, position.y, BtnW, position.height);
        if (GUI.Button(removeRect, "删除")) Remove(property);
    }

    /// <summary>元素路径形如 startPile.Array.data[3]，取出它所在的数组属性和下标。</summary>
    static SerializedProperty Array(SerializedProperty element, out int index)
    {
        const string marker = ".Array.data[";
        int cut = element.propertyPath.LastIndexOf(marker, System.StringComparison.Ordinal);
        index = -1;
        if (cut < 0) return null;
        index = int.Parse(element.propertyPath.Substring(cut + marker.Length).TrimEnd(']'));
        return element.serializedObject.FindProperty(element.propertyPath.Substring(0, cut));
    }

    static void Copy(SerializedProperty element)
    {
        int index;
        SerializedProperty array = Array(element, out index);
        if (array == null) return;
        int value = array.GetArrayElementAtIndex(index).intValue;
        array.InsertArrayElementAtIndex(index + 1);
        array.GetArrayElementAtIndex(index + 1).intValue = value;
        Commit(element);
    }

    static void Remove(SerializedProperty element)
    {
        int index;
        SerializedProperty array = Array(element, out index);
        if (array == null) return;
        array.DeleteArrayElementAtIndex(index);
        Commit(element);
    }

    static void Commit(SerializedProperty element)
    {
        element.serializedObject.ApplyModifiedProperties();
        GUIUtility.ExitGUI();
    }
}
