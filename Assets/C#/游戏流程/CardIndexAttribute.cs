using UnityEngine;

/// <summary>
/// 标在 List&lt;int&gt; 上，检查器里把每个元素画成卡片下拉框：选项来自 CardManager.CardDefine（cards.json），
/// 存的是卡片下标，行尾带「复制 / 删除」。
/// 只影响编辑器绘制，不参与序列化：JsonUtility 与存档里仍然只是数字。
/// 用法：[CardIndex] public List<int> startPile;
/// </summary>
public class CardIndexAttribute : PropertyAttribute
{
}
