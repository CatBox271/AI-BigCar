using UnityEngine;

/// <summary>
/// Clock 的场景侧驱动。顶层独立 MonoBehaviour，嵌套类型 Unity 序列化不了。
/// </summary>
public class ClockRunner : MonoBehaviour
{
    void Update()
    {
        Clock.Tick(Time.deltaTime);
    }
}
