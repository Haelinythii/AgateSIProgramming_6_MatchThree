using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    //singleton for timemanager
    private static TimeManager instance = null;
    public static TimeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TimeManager>();
                if (instance == null)
                {
                    Debug.LogError("Instance for TimeManager not Found");
                }
            }
            return instance;
        }
    }

    public int gameDuration;
    private float gameTimer;

    private void Start()
    {
        gameTimer = 0;
    }

    private void Update()
    {
        if (GameFlowManager.Instance.IsGameOver) return;

        if(gameTimer > gameDuration)
        {
            GameFlowManager.Instance.GameOver();
            return;
        }
        gameTimer += Time.deltaTime;
    }

    public float GetRemainingTime()
    {
        return gameDuration - gameTimer;
    }
}
