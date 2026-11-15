using System.Collections;
using System.Collections.Generic;
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

    public EffectKind kind;
    private EffectKind last_kind = EffectKind.non;
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

        }
        //get
        if (text != null && textTransformEditor.NoAim()) textTransformEditor.SetAim(text.transform);
        if (pic != null && picTransformEditor.NoAim()) picTransformEditor.SetAim(pic.transform);
        //set
        if (text != null) textTransformEditor.ApplyAim();
        if (pic != null) picTransformEditor.ApplyAim();
        //save

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
