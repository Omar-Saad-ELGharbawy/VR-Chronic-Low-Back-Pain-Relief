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

    public Transform avatarSpine;

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
            LeftHandMoving();
            RightHandMoving();
            UpperBodyMoving();

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

        transform.rotation = Quaternion.Euler(hipsDepthDiff * 20, 0, -hipsHeightDiff * 150); // Rotate the object in local space

        Debug.Log($"Hip Points Height Difference : {hipsHeightDiff}");
        Debug.Log($"Hip Points Depth Difference : {hipsDepthDiff}");
        Debug.Log("#################################");
    }

    void LeftHandMoving()
    {

        Dictionary<string, float> leftHand = GetLandmarkData(16, landmarks);

        // Map the x-value to the new range
        float mappedX = MapValue(leftHand["x"], 0, 1, 1.35f, 0);
        float mappedY = MapValue(leftHand["y"], 0, 1, 4, 2);

        // Update the newLeftHandPosition
        Vector3 newLeftHandPosition = new Vector3(mappedX, mappedY, leftHandTarget.position.z);
        leftHandTarget.position = newLeftHandPosition;

        Debug.Log("Left Hand Position: " + newLeftHandPosition);

    }

    void RightHandMoving()
    {

        Dictionary<string, float> rightHand = GetLandmarkData(15, landmarks);

        // Map the x-value to the new range
        float mappedX = MapValue(rightHand["x"], 0, 1, 0, -1.35f);
        float mappedY = MapValue(rightHand["y"], 0, 1, 4, 2);

        // Update the newLeftHandPosition
        Vector3 newrightHandPosition = new Vector3(mappedX, mappedY, rightHandTarget.position.z);
        rightHandTarget.position = newrightHandPosition;

        Debug.Log("Right Hand Position: " + newrightHandPosition);

    }

    void UpperBodyMoving()
    {

        float hipsMedian = (GetLandmarkData(23, landmarks)["x"] + GetLandmarkData(24, landmarks)["x"]) / 2;
        float shoulderMedian = (GetLandmarkData(11, landmarks)["x"] + GetLandmarkData(12, landmarks)["x"]) / 2;

        float mediansDiff = hipsMedian - shoulderMedian;

        avatarSpine.localRotation = Quaternion.Euler(0, -mediansDiff * 200, 0);
    }

    // Function to map values from one range to another
    float MapValue(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return (value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin;
    }

    void StartGame()
    {
        isGameStarted = true;
        startPanel.SetActive(false);
        Time.timeScale = 1;
        isTimerActive = true;
    }

}

