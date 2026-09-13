using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishWhenAudioDone : MonoBehaviour
{
    public AudioSource audioSource;

    public static GameObject StaticAudio;

    public bool Father;

    static readonly List<VanishWhenAudioDone> Pool = new List<VanishWhenAudioDone>();
    static Transform poolRoot;
    bool pooled;

    private void Awake()
    {
        if (Father)
        {
            Father = false;
            StaticAudio = gameObject;
            poolRoot = transform.parent != null ? transform.parent : transform;
        }
    }

    /// <summary>
    /// 播放一次性音效并在播完后自动回收，替代各处“Instantiate + 设 clip/volume + Play + 播完自毁”。
    /// 返回 AudioSource 供调用方继续调整；StaticAudio 未就绪或 clip 为空时返回 null。
    /// 对象仍然挂在传入的 parent 下，所以 parent 被销毁时声音依旧会被一起切断（与原行为一致）。
    /// </summary>
    public static AudioSource Play(Transform parent, AudioClip clip, float volume)
    {
        return PlayInternal(parent, clip, volume, false, 1f);
    }

    /// <summary>同上，但额外设置 pitch（Balloon 需要）。不传就不动模板上的 pitch。</summary>
    public static AudioSource Play(Transform parent, AudioClip clip, float volume, float pitch)
    {
        return PlayInternal(parent, clip, volume, true, pitch);
    }

    static AudioSource PlayInternal(Transform parent, AudioClip clip, float volume, bool setPitch, float pitch)
    {
        if (StaticAudio == null || clip == null) return null;

        VanishWhenAudioDone v = null;
        while (Pool.Count > 0)
        {
            int last = Pool.Count - 1;
            v = Pool[last];
            Pool.RemoveAt(last);
            if (v != null) break;   // 随载具一起被销毁的池对象会变成假空，直接丢弃
            v = null;
        }

        if (v == null)
        {
            GameObject go = Instantiate(StaticAudio, parent);
            v = go.GetComponent<VanishWhenAudioDone>();
            v.pooled = true;
            if (v.audioSource == null)
            {
                AudioSource a;
                if (v.TryGetComponent(out a)) v.audioSource = a;
            }
        }
        else
        {
            v.transform.SetParent(parent, false);
            v.gameObject.SetActive(true);
        }

        v.transform.localPosition = Vector3.zero;
        v.transform.localEulerAngles = Vector3.zero;

        v.audioSource.clip = clip;
        v.audioSource.volume = volume;
        if (setPitch) v.audioSource.pitch = pitch;
        v.audioSource.loop = false;
        v.audioSource.Play();
        return v.audioSource;
    }

    private void Update()
    {
        if (audioSource.clip == null) return;
        if (audioSource.isPlaying) return;

        audioSource.clip = null;
        if (!pooled)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
        if (poolRoot != null) transform.SetParent(poolRoot, false);
        Pool.Add(this);
    }
}
