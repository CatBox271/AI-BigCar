using UnityEditor;
using UnityEngine;

/// <summary>
/// BlockIndexAttribute 的绘制器：把 int 画成道具下拉框，选项来自 GameManager.BlockLibrary。
/// 下拉显示「下标 + 中文名 + 代号」，但写回字段的只有下标。
/// 放在 Assets/C#/Editor 下，随 Assembly-CSharp-Editor 编译，不会进包体。
/// </summary>
[CustomPropertyDrawer(typeof(BlockIndexAttribute))]
public class BlockIndexDrawer : PropertyDrawer
{
    static string[] options;
    static int cachedCount = -1;
    static string cachedLast = "";

    /// <summary>按 BlockLibrary 生成下拉文本；表变了自动重建。</summary>
    public static string[] BuildOptions()
    {
        var lib = GameManager.BlockLibrary;
        string last = lib.Count > 0 ? lib[lib.Count - 1].code_name : "";
        if (options != null && cachedCount == lib.Count && cachedLast == last) return options;

        cachedCount = lib.Count;
        cachedLast = last;
        options = new string[lib.Count];
        for (int i = 0; i < lib.Count; i++)
        {
            var b = lib[i];
            string extra = "";
            // 0 号是占位的“空气”，没预制体是正常的
            // 用 File.Exists 而不是 AssetDatabase.LoadAssetAtPath：后者会把预制体真正加载一遍，
            // 触发 Battery / OwnCar 的 OnValidate 警告，而且每次重编选项都要跑 57 次
            if (i > 0 && !System.IO.File.Exists("Assets/Resources/GameObject/" + b.code_name + ".prefab"))
            {
                extra = "  （无预制体）";
            }
            options[i] = i + "  " + b.chinese_name + " (" + b.code_name + ")" + extra;
        }
        return options;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.HelpBox(position, "BlockIndex 只能标在 int 字段上，当前是 " + property.propertyType, MessageType.Error);
            return;
        }

        string[] list = BuildOptions();

        // 越界值（例如将来从 Json 读到旧数据）不静默改写，显示为空等用户重选
        int cur = property.intValue;
        if (cur < 0 || cur >= list.Length) cur = -1;

        EditorGUI.BeginProperty(position, label, property);
        bool mixed = EditorGUI.showMixedValue;
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        // 这个版本的 EditorGUI.Popup 没有 GUIContent 重载，先画标签再画下拉
        Rect fieldRect = EditorGUI.PrefixLabel(position, label);
        EditorGUI.BeginChangeCheck();
        int picked = EditorGUI.Popup(fieldRect, cur, list);
        if (EditorGUI.EndChangeCheck() && picked != cur)
        {
            property.intValue = picked;
        }
        EditorGUI.showMixedValue = mixed;
        EditorGUI.EndProperty();
    }
}
