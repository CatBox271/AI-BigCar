using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 道具缩略图生成器：把 Resources/GameObject 下的预制体离屏渲染成清晰 PNG，写入 Resources/Sprite。
/// 不用 AssetPreview：它只有 128px 且是异步的，常常第一次返回 null。
/// </summary>
public static class ExportPreviewTool
{
    const string Menu = "Tools/道具缩略图/";

    [MenuItem(Menu + "生成器...")]
    static void Open()
    {
        BlockThumbnailWindow.Open();
    }

    [MenuItem(Menu + "一键更新全部图标")]
    static void OneKey()
    {
        BlockThumbnailWindow.RunBatch(new BlockThumbnailWindow.Settings());
    }
}

/// <summary>缩略图生成参数。改这里就能定制，不用动渲染代码。</summary>
public class BlockThumbnailWindow : EditorWindow
{
    /// <summary>预览专用层：渲染时把待拍对象挪到这层，相机只渲这层，避免把已打开场景一起拍进去。</summary>
    const int PreviewLayer = 30;

    public class Settings
    {
        public string sourceFolder = "Assets/Resources/GameObject";
        public string outputFolder = "Assets/Resources/Sprite";
        public int resolution = 512;     // 128 / 256 / 512 / 1024
        public int superSample = 2;      // 1 / 2 / 4
        public int msaa = 4;             // 0 / 2 / 4 / 8
        public float padding = 0.05f;    // 四周留白占比
        public bool transparentBackground = true;
        public Color backgroundColor = new Color(0f, 0f, 0f, 0f);
        public int scope = 0;            // 0 全部 1 只处理选中 2 只补缺失
        public bool overwrite = true;
        public bool onlyLibrary = true;   // 只处理 BlockLibrary 里登记过的道具
        public bool writeSpriteImportSettings = true;
        public float pixelsPerUnit = 256f; // 跟现有图标一致，改了 UI 里 SetNativeSize 的尺寸会变
    }

    Settings settings = new Settings();
    Vector2 scroll;
    List<string> prefabs = new List<string>();

    public static void Open()
    {
        var w = GetWindow<BlockThumbnailWindow>("道具缩略图");
        w.minSize = new Vector2(480f, 460f);
        w.Show();
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("目录", EditorStyles.boldLabel);
        settings.sourceFolder = EditorGUILayout.TextField("预制体目录", settings.sourceFolder);
        settings.outputFolder = EditorGUILayout.TextField("图标目录", settings.outputFolder);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("画质", EditorStyles.boldLabel);
        settings.resolution = EditorGUILayout.IntPopup("分辨率", settings.resolution,
            new[] { "128", "256", "512", "1024" }, new[] { 128, 256, 512, 1024 });
        settings.superSample = EditorGUILayout.IntPopup("超采样", settings.superSample,
            new[] { "1x", "2x", "4x" }, new[] { 1, 2, 4 });
        settings.msaa = EditorGUILayout.IntPopup("MSAA", settings.msaa,
            new[] { "关", "2x", "4x", "8x" }, new[] { 0, 2, 4, 8 });
        settings.padding = EditorGUILayout.Slider("边距", settings.padding, 0f, 0.4f);
        settings.transparentBackground = EditorGUILayout.Toggle("透明背景", settings.transparentBackground);
        if (!settings.transparentBackground)
            settings.backgroundColor = EditorGUILayout.ColorField("背景颜色", settings.backgroundColor);
        settings.pixelsPerUnit = EditorGUILayout.FloatField("Sprite PPU", settings.pixelsPerUnit);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("范围", EditorStyles.boldLabel);
        settings.scope = EditorGUILayout.Popup("处理范围", settings.scope,
            new[] { "全部预制体", "只处理选中的预制体", "只补缺失的图标" });
        settings.overwrite = EditorGUILayout.Toggle("覆盖已有图标", settings.overwrite);
        settings.onlyLibrary = EditorGUILayout.Toggle(
            new GUIContent("只处理表内道具", "只处理 GameManager.BlockLibrary 里登记过的道具，跳过 Hinge 这类表外预制体"),
            settings.onlyLibrary);
        settings.writeSpriteImportSettings = EditorGUILayout.Toggle("写入 Sprite 导入设置", settings.writeSpriteImportSettings);

        EditorGUILayout.Space();
        if (GUILayout.Button("扫描", GUILayout.Height(24f)))
        {
            prefabs = CollectPrefabs(settings);
            Debug.Log("[缩略图] 扫描到 " + prefabs.Count + " 个预制体：" + settings.sourceFolder);
        }
        EditorGUILayout.LabelField("待处理：" + prefabs.Count + " 个" + (prefabs.Count == 0 ? "（先点扫描）" : ""));

        if (prefabs.Count > 0)
        {
            int miss = 0;
            EditorGUILayout.LabelField("还没有图标的：", EditorStyles.miniBoldLabel);
            foreach (var p in prefabs)
            {
                string n = Path.GetFileNameWithoutExtension(p);
                if (!File.Exists(ResolveOutput(settings.outputFolder, n)))
                {
                    miss++;
                    EditorGUILayout.LabelField("    " + n);
                }
            }
            if (miss == 0) EditorGUILayout.LabelField("    （无，全部已有图标）");
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("生成 / 覆盖", GUILayout.Height(30f)))
        {
            RunBatch(settings);
            prefabs = CollectPrefabs(settings);
        }

        EditorGUILayout.HelpBox("建议：分辨率 512、超采样 2x、MSAA 4。PPU 固定 256 与现有图标一致，" +
            "否则 BlockChoose.SetNativeSize() 的图标尺寸会变。Overlay Canvas 的 UI（如 IC）不会渲染进缩略图。",
            MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>批量生成。菜单「一键更新全部图标」走的也是这里。</summary>
    public static void RunBatch(Settings s, System.Action<string> onItem = null)
    {
        var list = CollectPrefabs(s);
        if (list.Count == 0)
        {
            Debug.LogWarning("[缩略图] 没有需要处理的预制体：" + s.sourceFolder);
            return;
        }

        int done = 0, skipped = 0;
        var fails = new List<string>();
        var written = new List<string>();

        // 关键：批处理期间关掉自动打图集。否则每张图导入都会重打一次 Sprite Atlas，
        // 实测 56 张要 366s，其中渲染只占几十毫秒。
        SpritePackerMode prevPackMode = EditorSettings.spritePackerMode;
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
        try
        {
            // 第一段：只写 PNG 字节，导入被 StartAssetEditing 推迟
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string path = list[i];
                    string name = Path.GetFileNameWithoutExtension(path);
                    string outPath = ResolveOutput(s.outputFolder, name);

                    if (!s.overwrite && File.Exists(outPath)) { skipped++; continue; }
                    if (EditorUtility.DisplayCancelableProgressBar("生成道具缩略图",
                            name + "  (" + (i + 1) + "/" + list.Count + ")", (float)i / list.Count))
                    {
                        Debug.LogWarning("[缩略图] 已取消，已完成 " + done + " 张");
                        break;
                    }

                    if (onItem != null) onItem(name);

                    string error;
                    Texture2D tex = RenderPrefab(path, s, out error);
                    if (tex == null) { skipped++; fails.Add(name + "  ←  " + error); continue; }

                    try { SavePng(tex, outPath); }
                    finally { Object.DestroyImmediate(tex); }
                    written.Add(outPath);
                    done++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // 第二段：导入完成后再统一套导入设置（新文件的 importer 此时才存在）
            if (s.writeSpriteImportSettings && written.Count > 0)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var p in written) ApplyImportSettings(p, s);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }
        }
        finally
        {
            EditorSettings.spritePackerMode = prevPackMode;
            EditorUtility.ClearProgressBar();
        }

        // 不再调用 SaveAssets：那会把 Battery / NuclearBattery 这类被 OnValidate 弄脏的预制体一并存盘
        AssetDatabase.Refresh();
        Debug.Log("[缩略图] 生成 " + done + " 张，跳过 " + skipped + " 张 → " + s.outputFolder
            + (fails.Count > 0 ? "\n跳过的原因：\n  " + string.Join("\n  ", fails) : ""));
    }

    /// <summary>收集要处理的预制体路径。</summary>
    public static List<string> CollectPrefabs(Settings s)
    {
        var result = new List<string>();

        if (s.scope == 1)
        {
            foreach (var g in Selection.assetGUIDs)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)) result.Add(p);
            }
            result.Sort(System.StringComparer.Ordinal);
            return result;
        }

        if (!AssetDatabase.IsValidFolder(s.sourceFolder)) return result;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { s.sourceFolder }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (!p.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)) continue;
            string bareName = Path.GetFileNameWithoutExtension(p);
            if (s.onlyLibrary && GameManager.GetBlockIndexByCode(bareName) < 0) continue;
            if (s.scope == 2 && File.Exists(ResolveOutput(s.outputFolder, bareName))) continue;
            result.Add(p);
        }
        result.Sort(System.StringComparer.Ordinal);
        return result;
    }

    /// <summary>输出路径：优先复用目录里大小写不同但同名的旧图标（engine / Panzer / H-BLock / Shield）。</summary>
    public static string ResolveOutput(string outFolder, string prefabName)
    {
        string exact = outFolder + "/" + prefabName + ".png";
        if (File.Exists(exact)) return exact;
        if (Directory.Exists(outFolder))
        {
            foreach (var f in Directory.GetFiles(outFolder, "*.png"))
            {
                if (Path.GetFileNameWithoutExtension(f).Equals(prefabName, System.StringComparison.OrdinalIgnoreCase))
                    return f.Replace(System.IO.Path.DirectorySeparatorChar, '/');
            }
        }
        return exact;
    }

    /// <summary>离屏渲染一个预制体，返回目标分辨率的 Texture2D；失败返回 null 并给出原因。</summary>
    static Texture2D RenderPrefab(string prefabPath, Settings s, out string error)
    {
        error = null;
        // 预览场景 / LoadPrefabContents 里的对象普通相机渲不到（实测 0 像素），
        // 所以在活动场景临时实例化，用 HideAndDontSave + 独立层隔离，渲完立刻销毁。
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (asset == null) { error = "预制体加载失败"; return null; }
        var targetScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(asset, targetScene);
        if (root == null) { error = "预制体实例化失败"; return null; }

        GameObject camGo = null;
        RenderTexture big = null, rtFinal = null;
        Texture2D bigTex = null, cropped = null;
        RenderTexture prevActive = RenderTexture.active;
        try
        {
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                tr.gameObject.layer = PreviewLayer;
                tr.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            foreach (var a in root.GetComponentsInChildren<Animator>(true)) a.enabled = false;
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var em = ps.emission;
                em.enabled = false;
            }

            Bounds bounds;
            if (!TryGetBounds(root, out bounds)) { error = "没有可渲染的 Renderer"; return null; }

            int res = Mathf.Clamp(s.resolution, 16, 2048);
            int ss = Mathf.Clamp(s.superSample, 1, 4);
            int msaa = Mathf.Clamp(s.msaa, 0, 8);
            int sizeBig = res * ss;

            camGo = new GameObject("ThumbCam", typeof(Camera));
            camGo.hideFlags = HideFlags.HideAndDontSave;
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = s.transparentBackground ? new Color(0f, 0f, 0f, 0f) : s.backgroundColor;
            cam.cullingMask = 1 << PreviewLayer;
            cam.allowHDR = false;
            cam.allowMSAA = true;
            cam.aspect = 1f;
            cam.nearClipPlane = 0.01f;

            // 先放宽取景，后面按真实不透明像素自动裁边，避免被隐藏的辅助精灵带偏
            float halfH = Mathf.Max(bounds.extents.y, bounds.extents.x);
            cam.orthographicSize = Mathf.Max(halfH, 0.0001f) * 1.4f;
            float dist = Mathf.Max(10f, bounds.size.magnitude * 2f);
            camGo.transform.position = bounds.center + new Vector3(0f, 0f, -dist);
            camGo.transform.rotation = Quaternion.identity;
            cam.farClipPlane = dist * 3f;

            big = new RenderTexture(sizeBig, sizeBig, 24, RenderTextureFormat.ARGB32);
            big.antiAliasing = msaa <= 0 ? 1 : msaa;
            big.filterMode = FilterMode.Bilinear;
            cam.targetTexture = big;
            cam.Render();

            RenderTexture.active = big;
            bigTex = new Texture2D(sizeBig, sizeBig, TextureFormat.RGBA32, false);
            bigTex.ReadPixels(new Rect(0f, 0f, sizeBig, sizeBig), 0, 0);
            bigTex.Apply();

            int bx, by, side;
            if (!AlphaSquare(bigTex, s.padding, out bx, out by, out side))
            {
                error = "渲染结果全透明（可能只有隐藏物体）";
                return null;
            }

            cropped = new Texture2D(side, side, TextureFormat.RGBA32, false);
            cropped.SetPixels(bigTex.GetPixels(bx, by, side, side));
            cropped.Apply();

            rtFinal = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32);
            rtFinal.filterMode = FilterMode.Bilinear;
            Graphics.Blit(cropped, rtFinal);

            RenderTexture.active = rtFinal;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0f, 0f, res, res), 0, 0);
            tex.Apply();
            return tex;
        }
        catch (System.Exception e)
        {
            error = e.GetType().Name + ": " + e.Message;
            return null;
        }
        finally
        {
            RenderTexture.active = prevActive;
            if (camGo != null) Object.DestroyImmediate(camGo);
            if (big != null) { big.Release(); Object.DestroyImmediate(big); }
            if (rtFinal != null) { rtFinal.Release(); Object.DestroyImmediate(rtFinal); }
            if (bigTex != null) Object.DestroyImmediate(bigTex);
            if (cropped != null) Object.DestroyImmediate(cropped);
            if (root != null) Object.DestroyImmediate(root);
        }
    }

    /// <summary>不透明区域的方形包围盒（含留白），坐标系与 Texture2D 一致。</summary>
    static bool AlphaSquare(Texture2D tex, float padding, out int x, out int y, out int side)
    {
        x = y = side = 0;
        Color32[] px = tex.GetPixels32();
        int w = tex.width, h = tex.height;
        int minX = w, minY = h, maxX = -1, maxY = -1;
        for (int iy = 0; iy < h; iy++)
        {
            int row = iy * w;
            for (int ix = 0; ix < w; ix++)
            {
                if (px[row + ix].a <= 8) continue;
                if (ix < minX) minX = ix;
                if (ix > maxX) maxX = ix;
                if (iy < minY) minY = iy;
                if (iy > maxY) maxY = iy;
            }
        }
        if (maxX < 0) return false;

        int bw = maxX - minX + 1;
        int bh = maxY - minY + 1;
        side = Mathf.CeilToInt(Mathf.Max(bw, bh) * (1f + 2f * Mathf.Max(0f, padding)));
        side = Mathf.Clamp(side, 1, Mathf.Min(w, h));
        x = Mathf.Clamp((minX + maxX) / 2 - side / 2, 0, w - side);
        y = Mathf.Clamp((minY + maxY) / 2 - side / 2, 0, h - side);
        return true;
    }

    /// <summary>优先用 SpriteRenderer 的包围盒；没有就退回所有 Renderer，跳过零尺寸的。</summary>
    static bool TryGetBounds(GameObject root, out Bounds bounds)
    {
        bounds = new Bounds(root.transform.position, Vector3.zero);
        bool inited = false;

        foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (r == null || !r.enabled) continue;
            if (r.sprite == null) continue;
            if (!inited) { bounds = r.bounds; inited = true; }
            else bounds.Encapsulate(r.bounds);
        }

        if (!inited)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || !r.enabled) continue;
                if (r.bounds.size.sqrMagnitude <= 0f) continue;
                if (!inited) { bounds = r.bounds; inited = true; }
                else bounds.Encapsulate(r.bounds);
            }
        }
        return inited && bounds.size.sqrMagnitude > 0f;
    }

    static void SavePng(Texture2D tex, string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    /// <summary>把图标导入设置成和现有图标一致：Sprite、PPU 256、关 mipmap、不压缩。</summary>
    static void ApplyImportSettings(string path, Settings s)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = s.pixelsPerUnit > 0f ? s.pixelsPerUnit : 256f;
        ti.filterMode = FilterMode.Bilinear;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.maxTextureSize = 2048;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }
}
