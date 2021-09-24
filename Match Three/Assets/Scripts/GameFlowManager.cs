using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    private static GameFlowManager instance = null;
    public static GameFlowManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameFlowManager>();

                if (instance == null)
                {
                    Debug.LogError("Instance for GameFlowManager not Found");
                }
            }
            return instance;
        }
    }
    
    private bool isGameOver = false;
    public bool IsGameOver { get => isGameOver; }

    public UIGameOver uiGameOver;

    private void Start()
    {
        isGameOver = false;
    }

    public void GameOver()
    {
        isGameOver = true;
        ScoreManager.Instance.SetHighscore();
        uiGameOver.Show();
    }
}
