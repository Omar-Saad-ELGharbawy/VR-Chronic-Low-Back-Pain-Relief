using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Generic;

public class Pose : MonoBehaviour
{

    UdpClient server;
    const int port = 11000;
    Thread thread1;

    float[] landmarks = new float[132];

    float hipsHeightDiff;
    float hipsDepthDiff;

    private void Start()
    {
        server = new UdpClient(port);
        thread1 = new Thread(ReceiveData);
        thread1.Start();
    }


    private void Update()
    {
        if (thread1.IsAlive)
        {
            RotateTable();
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
        hipsHeightDiff = GetLandmarkData(23, landmarks)["y"] - GetLandmarkData(24, landmarks)["y"];

        // Get the depth difference between the hips
        hipsDepthDiff = GetLandmarkData(23, landmarks)["z"] - GetLandmarkData(24, landmarks)["z"];

        transform.rotation = Quaternion.Euler(hipsHeightDiff * 100, 0, hipsDepthDiff * 20); // Rotate the object in local space

        Debug.Log($"Waist Points Height Difference : {hipsHeightDiff}");
        Debug.Log($"Waist Points Depth Difference : {hipsDepthDiff}");
        Debug.Log("#################################");
    }
}

