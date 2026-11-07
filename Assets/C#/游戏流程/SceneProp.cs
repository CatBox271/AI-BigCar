using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneProp : MonoBehaviour
{
    public static SceneProp Instance;
    public bool VideoMode;
    void Awake()
    {
        Instance = this;
    }
}
