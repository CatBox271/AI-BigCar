using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CodeLibraryRefreshTool : AssetPostprocessor
{
    private const string Menu = "Tools/代码百科/";

    [MenuItem(Menu + "强制刷新数据")]
    static void ManualRefresh()
    {
        AssetDatabase.ImportAsset("Assets/Resources/Data/CodeLibrary.json", ImportAssetOptions.ForceUpdate);
        RefreshData();
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith("CodeLibrary.json"))
            {
                RefreshData();
                break;
            }
        }
    }

    public static void RefreshData()
    {
        GameManager.ClearCodeLibrary();
        var count = GameManager.CodeLibrary.Count;

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var loadPages = typeof(CodeInform).GetMethod("LoadPages", flags);
        var render = typeof(CodeInform).GetMethod("Render", flags);
        var currentKindProp = typeof(CodeInform).GetProperty("CurrentKind", flags);

        var informs = Resources.FindObjectsOfTypeAll<CodeInform>();
        foreach (var inform in informs)
        {
            if (inform == null || EditorUtility.IsPersistent(inform)) continue;
            loadPages?.Invoke(inform, null);
            string kind = currentKindProp?.GetValue(inform) as string ?? "";
            if (!string.IsNullOrEmpty(kind))
                render?.Invoke(inform, new object[] { kind });
        }
        Debug.Log($"[代码百科] 已刷新，共载入 {count} 条指令。");
    }
}
