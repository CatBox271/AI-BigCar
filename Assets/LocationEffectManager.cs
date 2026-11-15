using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectKind
{
    non,//default
    vn,//vecihle num add
    hp,//HP percent add
    vel,//velocity percent add
    hit,//hit percent add
    size,//size percent add
    adv,// advent effect rate percent add
}

[System.Serializable]
public class LocationEffect
{

    [System.Serializable]
    public class CarEffect//上层先排好序，先forever后！forever，然后在小组里按EffectKind的顺序排序执行，对外说是先加再乘，percent乘算
    {
        public EffectKind effectKind;
        public float effectNum;
        public bool forever;
        public void Apply(OwnCar car)
        {
            switch (effectKind)
            {

            }
        }
        public void Cancel(OwnCar car)
        {
            switch (effectKind)
            {

            }
        }
    }
    public string pic_name;
    public float rarity;//稀有权重反比，比如1是1,2是1/2。把所有卡池想象为一个由稀有权重线段拼成的线，1的稀有度就占据1的长度，2就占据1/2。生成时从线段上任取一点，这个点属于哪个就是取那个。
    public float circle;//位置生成的“圈”级可能性，比如0是高概率在中间三个,1是3个向相邻对角扩散并排除哪3个的一个空心长方形，2是最外圈。
    public string describe;
    public List<CarEffect> effects;

}
[System.Serializable]
public class LocationEffectTable
{
    public List<LocationEffect> effects = new();
}
public class LocationEffectManager : MonoBehaviour
{
    public const string LocationEffectPath = "Data/LocationEffect";

    // 表体内存。对外统一走 EffectModels 属性，保证第一次访问时就已从 JSON 载入。
    static readonly List<LocationEffect> effectModels = new();

    public static List<LocationEffect> EffectModels//可以抽到的可能模版全集
    {
        get
        {
            EnsureEffectModels();
            return effectModels;
        }
    }

    static void EnsureEffectModels()
    {
        if (effectModels.Count > 0) return;
        var asset = Resources.Load<TextAsset>(LocationEffectPath);
        var table = JsonUtility.FromJson<LocationEffectTable>(asset.text);
        effectModels.Clear();
        effectModels.AddRange(table.effects);
    }

    public static void ClearEffectModels()//LocationEffect.json 重新导入时清空内存表
    {
        effectModels.Clear();
    }

    public OwnCar car;
    public GameObject effectPic;
    void Awake()
    {
        car.BeforeSend += OnSend;
        car.SendingFinish += OnRestart;
        //表不在这里载入：EffectModels 属性第一次访问时自己读 JSON；
        //编辑器里改完表由 LocationEffectReloader 清缓存，下一帧访问就是新的。
    }
    private void OnSend()
    {
        //应用效果给car
    }
    private void OnRestart()
    { 
        //随机生成的效果
    }
}

#if UNITY_EDITOR
public class DataTableReloader : UnityEditor.AssetPostprocessor//表一被导入就清对应内存表，省得读到旧数据
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            if (path.EndsWith("Data/LocationEffect.json")) LocationEffectManager.ClearEffectModels();
            if (path.EndsWith("Data/cards.json")) CardManager.ClearCardDefine();
            if (path.EndsWith("Data/BlockLibrary.json")) GameManager.ClearBlockLibrary();
        }
    }
}
#endif
