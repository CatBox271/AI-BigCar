using System.IO;
using UnityEditor;

public class Utf8BomStamp : AssetPostprocessor//Unity 新建的脚本不带 BOM，导入时补上，保持全项目 UTF-8 + BOM
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (!path.EndsWith(".cs")) continue;
            byte[] data = File.ReadAllBytes(path);
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) continue;
            byte[] withBom = new byte[data.Length + 3];
            withBom[0] = 0xEF;
            withBom[1] = 0xBB;
            withBom[2] = 0xBF;
            data.CopyTo(withBom, 3);
            File.WriteAllBytes(path, withBom);
        }
    }
}
