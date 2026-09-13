using UnityEngine;
using System.IO;

public static class FileUtils
{
    /// <summary>
    /// 获取持久化路径下文件夹的所有文件
    /// </summary>
    /// <param name="relativeFolder">相对路径（如 "Saves/Images"）</param>
    /// <param name="searchPattern">搜索模式（如 "*.png"）</param>
    /// <param name="includeSubfolders">是否包含子文件夹</param>
    public static string[] GetFilesInPersistentFolder(string relativeFolder = "",
                                                     string searchPattern = "*",
                                                     bool includeSubfolders = false)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, relativeFolder);

        if (!Directory.Exists(fullPath))
            return new string[0];

        return Directory.GetFiles(
            fullPath,
            searchPattern,
            includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );
    }
}