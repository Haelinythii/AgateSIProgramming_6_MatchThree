using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundManager>();

                if (instance == null)
                {
                    Debug.LogError("Instance of SoundManager not Found");
                }
            }
            return instance;
        }
    }

    public AudioClip scoreNormal, scoreCombo, wrongMove, tap;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayScoreSound(bool isCombo)
    {
        if (isCombo)
        {
            audioSource.PlayOneShot(scoreCombo);
        }
        else
        {
            audioSource.PlayOneShot(scoreNormal);
        }
    }

    public void PlayWrongMoveSound()
    {
        audioSource.PlayOneShot(wrongMove);
    }

    public void PlayTapSound()
    {
        audioSource.PlayOneShot(tap);
    }
}
