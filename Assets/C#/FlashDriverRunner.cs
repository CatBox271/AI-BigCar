using UnityEngine;

/// <summary>
/// FlashDriver 的场景侧驱动。
/// 必须是顶层独立 MonoBehaviour：嵌套在静态类里 Unity 序列化不了，场景里会变成 Script 丢失的组件。
/// </summary>
public class FlashDriverRunner : MonoBehaviour
{
    void Update()
    {
        FlashDriver.Tick(Time.deltaTime);
    }
}
