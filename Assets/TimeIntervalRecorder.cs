using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TimeIntervalRecorder : MonoBehaviour
{
    private float lastPressTime = -1f;
    private List<float> timeIntervals = new List<float>();
    private string filePath;

    void Start()
    {
        // 设置文件保存路径（在项目根目录的Logs文件夹下）
        string directoryPath = Path.Combine(Application.dataPath, "../Logs");
        Directory.CreateDirectory(directoryPath); // 确保目录存在
        filePath = Path.Combine(directoryPath, "KeyPressIntervals.txt");
        Debug.Log("日志文件将保存至: " + filePath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            RecordKeyPress();
        }
    }

    void RecordKeyPress()
    {
        float currentTime = Time.time;
        
        // 如果不是第一次按键
        if (lastPressTime > 0)
        {
            float interval = currentTime - lastPressTime;
            timeIntervals.Add(interval);
            Debug.Log($"按键间隔: {interval:F3}秒");
        }
        else
        {
            Debug.Log("开始记录时间间隔...");
        }
        
        lastPressTime = currentTime;
    }

    // 生成日志文件
    void GenerateLogFile()
    {
        if (timeIntervals.Count == 0)
        {
            Debug.LogWarning("没有记录任何时间间隔！");
            return;
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"按键时间间隔日志 - 生成时间: {System.DateTime.Now}");
            writer.WriteLine("=================================");
            
            for (int i = 0; i < timeIntervals.Count; i++)
            {
                writer.WriteLine($"间隔 #{i + 1}: {timeIntervals[i]:F3}秒");
            }
            
            writer.WriteLine("=================================");
            writer.WriteLine($"总按键次数: {timeIntervals.Count + 1}");
            writer.WriteLine($"总记录时间: {Time.time:F1}秒");
            writer.WriteLine($"平均间隔: {CalculateAverage():F3}秒");
        }
        
        Debug.Log($"已生成日志文件: {filePath}");
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(filePath);
        #endif
    }

    float CalculateAverage()
    {
        float total = 0f;
        foreach (float interval in timeIntervals)
        {
            total += interval;
        }
        return total / timeIntervals.Count;
    }

    // 退出时自动保存（可选）
    void OnApplicationQuit()
    {
        GenerateLogFile();
    }

    // 添加UI提示（按需使用）
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), "按下 K 键记录时间间隔");
        GUI.Label(new Rect(10, 40, 400, 30), $"已记录间隔: {timeIntervals.Count}次");
        if (timeIntervals.Count > 0)
        {
            GUI.Label(new Rect(10, 70, 400, 30), $"最后一次间隔: {timeIntervals[^1]:F3}秒");
        }
    }
}