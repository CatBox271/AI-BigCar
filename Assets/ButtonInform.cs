using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInform : MonoBehaviour
{
    [Tooltip("生成的Button种类")]
    public List<string> ButtonName;
    [Tooltip(" 0 开关，1数字，2三进制")]
    public List<int> ButtonState;
    [Tooltip("冷却时间")]
    public List<float> CD;
    [Tooltip("x 最小值，y最大值， z 1无限制 2 为有限制，3为循环,负数是整数")]
    public List<Vector3Int> NumLimite;
}
