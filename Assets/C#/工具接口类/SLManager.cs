using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public static class SLManager
{
    static string _persistentDataPath = "";

    static string persistentDataPath
    {
        get
        {
            if (_persistentDataPath == "") _persistentDataPath = Application.persistentDataPath;
            return _persistentDataPath;
        }
    }

    //Newtonsoft 默认连属性一起写，Vector3 的 normalized 会自引用死循环；
    //这里只认 public 实例字段，和 Unity 自己的序列化同一套口径（private 的 aim_tf 之类不会进存档）
    class FieldOnlyResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(System.Type objectType)
        {
            FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            List<MemberInfo> members = new();
            for (int i = 0; i < fields.Length; i++) members.Add(fields[i]);
            return members;
        }
    }

    static JsonSerializerSettings settings = new() { ContractResolver = new FieldOnlyResolver() };

    /// <summary>
    /// 导出任意对象到JSON文件
    /// </summary>
    public static string ExportToJson<T>(T data, string folderPath = "", string fileName = null, bool prettyPrint = true)
    {
        return Export(data, persistentDataPath, folderPath, fileName, prettyPrint);
    }

    /// <summary>
    /// 导出到 Assets/Resources 下，编辑器里改数据表用，运行时不要调
    /// </summary>
    public static string ExportToResources<T>(T data, string folderPath = "", string fileName = null, bool prettyPrint = true)
    {
        string path = Export(data, Path.Combine(Application.dataPath, "Resources"), folderPath, fileName, prettyPrint);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        return path;
    }

    static string Export<T>(T data, string rootPath, string folderPath, string fileName, bool prettyPrint)
    {
        if (data == null)
        {
            Debug.LogWarning("导出数据为空");
            return null;
        }

        try
        {
            string json = JsonConvert.SerializeObject(data, prettyPrint ? Formatting.Indented : Formatting.None, settings);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("JSON序列化失败");
                return null;
            }

            string actualFileName = string.IsNullOrEmpty(fileName) ? GenerateDefaultFileName() : fileName;
            string basePath = string.IsNullOrEmpty(folderPath) ? rootPath : Path.Combine(rootPath, folderPath);

            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

            string filePath = Path.Combine(basePath, AddJsonSuffix(actualFileName));
            File.WriteAllText(filePath, json, Encoding.UTF8);

            return filePath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导出失败: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    static string GenerateDefaultFileName()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"conversation_{timestamp}.json";
    }

    public static T ImportFromJson<T>(string folderPath, string fileName)
    {
        string path = PathOf(folderPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"文件不存在: {path}");
            return default;
        }

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("文件为空");
                return default;
            }

            T data = JsonConvert.DeserializeObject<T>(json, settings);
            if (data == null)
            {
                Debug.LogWarning("文件解析失败");
                return default;
            }

            Debug.Log($"<color=green>✓ {typeof(T).Name} 导入成功</color>\n路径: {path}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入失败: {e.Message}");
            return default;
        }
    }

    /// <summary>
    /// 读 Resources 里的 json 数据表，resourcesPath 不带后缀，如 "Data/cards"
    /// </summary>
    public static T ImportFromResources<T>(string resourcesPath)
    {
        var asset = Resources.Load<TextAsset>(resourcesPath);
        if (asset == null)
        {
            Debug.LogWarning($"Resources 里找不到: {resourcesPath}");
            return default;
        }

        try
        {
            T data = JsonConvert.DeserializeObject<T>(asset.text, settings);
            if (data == null)
            {
                Debug.LogWarning($"文件解析失败: {resourcesPath}");
                return default;
            }
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入失败: {e.Message}");
            return default;
        }
    }

    public static bool Exists(string folderPath, string fileName)
    {
        return File.Exists(PathOf(folderPath, fileName));
    }

    public static void Delete(string folderPath, string fileName)
    {
        string path = PathOf(folderPath, fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    static string PathOf(string folderPath, string fileName)
    {
        return Path.Combine(persistentDataPath, folderPath, AddJsonSuffix(fileName));
    }

    static string AddJsonSuffix(string fileName)
    {
        if (fileName.EndsWith(".json")) return fileName;
        return fileName + ".json";
    }
}
