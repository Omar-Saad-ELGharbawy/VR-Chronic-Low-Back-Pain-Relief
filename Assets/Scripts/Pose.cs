using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

public class Pose : MonoBehaviour
{
    UdpClient server;
    const int port = 11000;
    Thread thread1;

    float[] result = new float[132];


    private void Start()
    {
        server = new UdpClient(port);
        thread1 = new Thread(DoWork);
        thread1.Start();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            thread1.Abort();
        }

    }

    void DoWork()
    {
        while (true)
        {
            // Receive data from client
            IPEndPoint clientEndpoint = null;
            byte[] data = server.Receive(ref clientEndpoint);

            Buffer.BlockCopy(data, 0, result, 0, data.Length);

            Debug.Log(result[0]);

        }

    }
}

