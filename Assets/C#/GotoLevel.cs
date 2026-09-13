using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoLevel : MonoBehaviour
{
    public void Level(string levelname)
    {
        SceneManager.LoadScene(levelname);
    }
}
