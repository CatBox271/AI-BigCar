using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Propellers : MonoBehaviour,IPower,ION
{
    private Rigidbody2D rb;

    private float XScale;

    void Awake()
    {
        TryGetComponent(out rb);
        XScale = Rotate.localScale.x;
    }

    public float Force;
    public float MaxSpeed;
    public float AutoControl;

    public Transform Rotate;


    Heath h;
    void FixedUpdate()
    {
        if (OnSet)
        {
            Vector2 sp = rb.velocity;

            float MS = MaxSpeed;
            float F = Force;
            if (F > 0)
            {
                Vector2 ff = Mathf.Max(0.5f, (1 - Vector2.Dot(transform.up, sp) / MS)) * F * (Vector3.Angle(transform.up, Vector3.up) > AutoControl ? transform.up : Vector3.up);
                AnimSpeed = ff.magnitude;
                rb.AddForce(ff);
            }
            else
            {
                Vector2 ff = Mathf.Max(0.5f, (1 - Vector2.Dot(transform.up, -sp) / MS)) * F * (Vector3.Angle(transform.up, Vector3.up) > AutoControl ? transform.up : Vector3.up);

                AnimSpeed = ff.magnitude;
                rb.AddForce(ff);
            }
        }
        Sound(OnSet);
    }

    float AnimSpeed = 0;
    float angle = 0;
    private bool form;
    bool Video;
    private void Update()
    {
        if (!OnSet) return;


        angle += AnimSpeed * Time.deltaTime * 0.75f;

        Rotate.localScale = new Vector3(Mathf.Cos(angle) * XScale, Rotate.localScale.y, Rotate.localScale.z);
    }

    public AudioClip[] ac;
    public float volume;

    AudioSource audioSound;
    public void Sound(bool On)
    {
        if (ac.Length != 0)
        {
            if (audioSound == null)
            {
                if (VanishWhenAudioDone.StaticAudio != null)
                {
                    GameObject go = Instantiate(VanishWhenAudioDone.StaticAudio, transform);
                    go.transform.localPosition = new();
                    go.transform.localEulerAngles = new();
                    audioSound = go.GetComponent<AudioSource>();
                    audioSound.clip = ac[UnityEngine.Random.Range(0, ac.Length)];
                    audioSound.loop = true;
                    audioSound.Play();
                }
            }
            else
            {
                audioSound.volume = On ? volume : 0;
            }
        }
    }

    public bool IsOn()
    {
        return OnSet;
    }

    public void AddPower(float set)
    {
        if (Force > 0)
        {
            Force += set * 发动机乘数;
        }
        else
        {
            Force -= set * 发动机乘数;
        }
    }
    public float 发动机乘数 = 100f;

    public bool OnSet { get; set; }
    public float NumSet { get; set; }
}
