using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LEShow : MonoBehaviour
{
    [System.Serializable]
    public class TransformEditor
    {
        private Transform aim_tf;
        public Vector3 local_position = Vector3.zero;
        public Vector3 local_euler_angles = Vector3.zero;
        public Vector3 local_scale = Vector3.one;
        public void SetAim(Transform tf)
        {
            aim_tf = tf;
            local_position = tf.localPosition;
            local_euler_angles = tf.localEulerAngles;
            local_scale = tf.localScale;
        }
        public void ApplyAim()
        {
            aim_tf.localPosition = local_position;
            aim_tf.localEulerAngles = local_euler_angles;
            aim_tf.localScale = local_scale;
        }
        public bool NoAim() => aim_tf == null;
    }
    public class LET
    {
        public class TEXT_SP
        {
            public TransformEditor TEXT;
            public TransformEditor SP;
        }
        public Dictionary<EffectKind, TEXT_SP> lets;
        public TEXT_SP GetTF(EffectKind kind)
        {
            if (lets.TryGetValue(kind, out var tfe)) return tfe;
            else return null;
        }
    }

    public EffectKind kind;
    private EffectKind last_kind = EffectKind.non;
    public string text_content;
    public TextMeshProUGUI text;
    public SpriteRenderer pic;

    public TransformEditor textTransformEditor = new();
    public TransformEditor picTransformEditor = new();

    private void OnValidate()
    {
        if (last_kind != kind)
        {
            last_kind = kind;
            //load
            Load(false);
            LoadPic();
        }
        //get
        if (text != null && textTransformEditor.NoAim()) textTransformEditor.SetAim(text.transform);
        if (pic != null && picTransformEditor.NoAim()) picTransformEditor.SetAim(pic.transform);
        //set
        if (text != null) textTransformEditor.ApplyAim();
        if (pic != null) picTransformEditor.ApplyAim();
        //save
        RequestSave();
        //content
        text.text = text_content;
    }
    private string path = "Data/LET";
    private float save_delay = 0.3f;//连续拖动时，最后一次改动后隔这么久才写一次 json
    private static Dictionary<EffectKind, LET.TEXT_SP> letsCache;
    void Load(bool reuse = true)//reuse == false强制刷新
    {
        if (letsCache == null || !reuse)
        {
            var let = SLManager.ImportFromResources<LET>(path);
            if (let == null) return;
            letsCache = let.lets;
        }
        if (letsCache == null || !letsCache.TryGetValue(kind, out var sp)) return;
        SetEditor(textTransformEditor, text == null ? null : text.transform, sp.TEXT);
        SetEditor(picTransformEditor, pic == null ? null : pic.transform, sp.SP);
    }

    void LoadPic()
    {
        if (pic == null) return;
        Sprite sp = Resources.Load<Sprite>("LocationEffect/" + kind);
        if (sp != null) pic.sprite = sp;
    }

    void Save()
    {
        var let = SLManager.ImportFromResources<LET>(path);
        if (let == null) let = new LET();
        if (let.lets == null) let.lets = new();
        let.lets[kind] = new LET.TEXT_SP { TEXT = textTransformEditor, SP = picTransformEditor };
        SLManager.ExportToResources(let, Path.GetDirectoryName(path), Path.GetFileName(path));
        letsCache = let.lets;
    }

    //拖动 Inspector 时 OnValidate 每帧都会来，这里只节流 json 落盘，Transform 刷新照旧实时
    void RequestSave()
    {
#if UNITY_EDITOR
        pending = this;
        pending_save = UnityEditor.EditorApplication.timeSinceStartup + save_delay;
        UnityEditor.EditorApplication.update -= TickSave;
        UnityEditor.EditorApplication.update += TickSave;
#endif
    }

#if UNITY_EDITOR
    private static LEShow pending;
    private static double pending_save;

    private static void TickSave()
    {
        if (UnityEditor.EditorApplication.timeSinceStartup < pending_save) return;
        UnityEditor.EditorApplication.update -= TickSave;
        if (pending != null) pending.Save();
        pending = null;
    }
#endif

    //SetAim 会拿当前 transform 的数值覆盖字段，所以先绑定 aim 再把表里的数值写回去
    void SetEditor(TransformEditor editor, Transform tf, TransformEditor saved)
    {
        if (saved == null) return;
        if (tf != null) editor.SetAim(tf);
        editor.local_position = saved.local_position;
        editor.local_euler_angles = saved.local_euler_angles;
        editor.local_scale = saved.local_scale;
    }

    void Awake()
    {
        Load();
    }
}
