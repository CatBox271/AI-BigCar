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
            // 挂在 transform.parent 上：Balloon 本身马上就会被销毁，声音不能跟着一起没
            VanishWhenAudioDone.Play(transform.parent, ac[Random.Range(0, ac.Length)], volume, pitch);
        }
    }
}
