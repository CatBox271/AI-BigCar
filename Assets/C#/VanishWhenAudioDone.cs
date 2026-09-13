using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishWhenAudioDone : MonoBehaviour
{
    public AudioSource audioSource;

    public static GameObject StaticAudio;

    public bool Father;

    private void Awake()
    {
        if (Father)
        {
            Father = false;
            StaticAudio = gameObject;
        }
    }
    private void Update()
    {
        if (audioSource.clip != null)
        {
            if (!audioSource.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
