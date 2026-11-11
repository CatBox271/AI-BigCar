using UnityEngine;

/// <summary>
/// 标在 int 字段上，检查器里会画成道具下拉框。
/// 存的就是 GameManager.BlockLibrary 的下标（与旧 BlockIndex、存档里的数字索引同序）。
/// 只影响编辑器绘制，不参与序列化：JsonUtility 里仍然只是一个 int。
/// 用法：[BlockIndex] public int block_kind;
/// </summary>
public class BlockIndexAttribute : PropertyAttribute
{
}
