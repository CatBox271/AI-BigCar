using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public float value = 1;
    public float max_value;
    private void OnDestroy()
    {
        if(GameManager.gameState == GameManager.GameState.Start )Sound();
    }
    public AudioClip[] ac;
    public float volume;
    public float pitch;
    public void Sound()
    {
        if (ac.Length != 0)
        {
            GameObject go = Instantiate(VanishWhenAudioDone.StaticAudio, transform.parent);
            go.transform.localPosition = new();
            go.transform.localEulerAngles = new();
            AudioSource audio = go.GetComponent<AudioSource>();
            audio.clip = ac[Random.Range(0, ac.Length)];
            audio.volume = volume;
            audio.pitch = pitch;
            audio.Play();
        }
    }
}
