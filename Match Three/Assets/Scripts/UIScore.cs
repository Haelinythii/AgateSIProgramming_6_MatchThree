using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    public Text highscoreText;
    public Text currentscoreText;

    private void Update()
    {
        highscoreText.text = ScoreManager.Instance.Highscore.ToString();
        currentscoreText.text = ScoreManager.Instance.CurrentScore.ToString();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
