using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance = null;

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScoreManager>();

                if (instance == null)
                {
                    Debug.LogError("Instance for ScoreManager not Found");
                }
            }

            return instance;
        }
    }

    private static int highscore;

    public int tileRatio;
    public int comboRatio;

    private int currentScore;

    public int Highscore { get { return highscore; } }
    public int CurrentScore { get { return currentScore; } }

    private void Start()
    {
        ResetCurrentScore();
    }

    private void ResetCurrentScore()
    {
        currentScore = 0;
    }

    public void IncrementScore(int tileCount, int comboCount)
    {
        currentScore += (tileCount * tileRatio) * (comboCount * comboRatio);

        SoundManager.Instance.PlayScoreSound(comboCount > 1);
    }

    public void SetHighscore()
    {
        highscore = Mathf.Max(currentScore, highscore);
    }
}
