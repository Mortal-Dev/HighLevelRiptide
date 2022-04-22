using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
        Application.runInBackground = true;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }
}
