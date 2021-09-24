using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITime : MonoBehaviour
{
    public Text timerText;

    private void Update()
    {
        timerText.text = ConvertIntToTimeString(TimeManager.Instance.GetRemainingTime() + 1);
    }

    private string ConvertIntToTimeString(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        string timeInString = string.Format("{0} : {1}", minutes.ToString(), seconds.ToString());

        return timeInString;
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
