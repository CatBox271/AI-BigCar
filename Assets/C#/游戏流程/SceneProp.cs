using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneProp : MonoBehaviour
{
    public static SceneProp Instance;
    public GameMode gameMode;
    public GameManager.GameState StartGameState;
    void Awake()
    {
        Instance = this;
        GameManager.gameState = StartGameState; 
    }
    public enum GameMode
    { 
        level,//普通游戏模式
        video,//旧版对战视频录制模式
        card,//新版卡片对战模式
    }
}
