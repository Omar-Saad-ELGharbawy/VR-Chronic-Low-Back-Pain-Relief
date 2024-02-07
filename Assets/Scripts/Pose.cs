using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Generic;
using TMPro;

public class Pose : MonoBehaviour
{

    UdpClient server;
    const int port = 11000;
    Thread thread1;


    float[] landmarks = new float[132];

    public bool isGameStarted = false;
    public GameObject startPanel;

    public bool isTimerActive = false;
    public float currentTime;
    [SerializeField] TextMeshProUGUI stopWatchText;

    public Transform leftHandTarget;
    public Transform rightHandTarget;


    private void Start()
    {
        server = new UdpClient(port);
        thread1 = new Thread(ReceiveData);
        thread1.Start();
        Time.timeScale = 0;
    }


    private void Update()
    {
        if (!thread1.IsAlive) return;

        // start the game when the users cross their arms
        if (!isGameStarted)
        {
            float leftWristX = GetLandmarkData(16, landmarks)["x"];
            float rightWristX = GetLandmarkData(15, landmarks)["x"];

            if (leftWristX != 0 && Mathf.Abs(leftWristX - rightWristX) < 0.05)
            {
                StartGame();
            }
        }
        else
        {
            RotateTable();

            // Start the stopwatch
            if (!isTimerActive) return;
            currentTime += Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            stopWatchText.text = time.Minutes.ToString() + ":" + time.Seconds.ToString() + ":" + time.Milliseconds.ToString();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            thread1.Abort();
        }

    }

    Dictionary<string, float> GetLandmarkData(int index, float[] landmarks)
    {
        if (index >= 0 && index < landmarks.Length / 4)
        {
            int startIndex = index * 4;
            return new Dictionary<string, float>
            {
                {"x", landmarks[startIndex]},
                {"y", landmarks[startIndex + 1]},
                {"z", landmarks[startIndex + 2]},
                {"visibility", landmarks[startIndex + 3]}
            };
        }
        else
        {
            Console.WriteLine("Invalid landmark index.");
            return null;
        }
    }

    void ReceiveData()
    {
        while (true)
        {
            // Receive data from client
            IPEndPoint clientEndpoint = null;
            byte[] data = server.Receive(ref clientEndpoint);

            Buffer.BlockCopy(data, 0, landmarks, 0, data.Length);
        }
    }

    void RotateTable()
    {
        // Get the height difference between the hips
        float hipsHeightDiff = GetLandmarkData(23, landmarks)["y"] - GetLandmarkData(24, landmarks)["y"];

        // Get the depth difference between the hips
        float hipsDepthDiff = GetLandmarkData(23, landmarks)["z"] - GetLandmarkData(24, landmarks)["z"];

        transform.rotation = Quaternion.Euler(hipsHeightDiff * 150, 0, hipsDepthDiff * 20); // Rotate the object in local space

        Debug.Log($"Hip Points Height Difference : {hipsHeightDiff}");
        Debug.Log($"Hip Points Depth Difference : {hipsDepthDiff}");
        Debug.Log("#################################");
    }


    void StartGame()
    {
        isGameStarted = true;
        startPanel.SetActive(false);
        Time.timeScale = 1;
        isTimerActive = true;
    }

}

