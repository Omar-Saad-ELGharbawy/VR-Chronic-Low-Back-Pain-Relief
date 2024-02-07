using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameOver : MonoBehaviour
{
    Pose poseScript;
    [SerializeField] TextMeshProUGUI bestTimeText;

    private void Start()
    {
        poseScript = GameObject.Find("Table").GetComponent<Pose>();


        bestTimeText.text = GetBestTime();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (PlayerPrefs.GetFloat("BestTime") < poseScript.currentTime)
            {
                PlayerPrefs.SetFloat("BestTime", poseScript.currentTime);
            }
            poseScript.isTimerActive = false;
            Invoke(nameof(RestartGame), 2f);
        }
    }

    void RestartGame()
    {
        poseScript.currentTime = 0;
        poseScript.isGameStarted = false;
        poseScript.startPanel.SetActive(true);
        Time.timeScale = 0;
        transform.position = new Vector3(0, 1.5f, 0);
        bestTimeText.text = GetBestTime();
    }

    string GetBestTime()
    {
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0);
        TimeSpan time = TimeSpan.FromSeconds(bestTime);
        return "Best Time\n" + time.Minutes.ToString() + ":" + time.Seconds.ToString() + ":" + time.Milliseconds.ToString();
    }

}
