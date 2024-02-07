using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Generic;

public class Pose : MonoBehaviour
{
    float hip_slope;
    float vertical_slope;
    float slopeZX_median;
    float z_diff;
    float z_11;
    float z_23;


    UdpClient server;
    const int port = 11000;
    Thread thread1;

    float[] landmarks = new float[132];

    public float rotationSpeed = 10f;

    private void Start()
    {
        server = new UdpClient(port);
        thread1 = new Thread(DoWork);
        thread1.Start();
    }
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     thread1.Abort();
        // }
        // RotateTable(0.5f, 0.2f);
        RotateTable(hip_slope, z_diff);
        // Debug.Log($"Hip Slope between landmarks 23 and 25 (YX): {hip_slope}");
        // Debug.Log($"Vertical Slope Slope between landmarks 23 and 11 (ZY): {vertical_slope}");
        Debug.Log($"Z 11 : {z_11}");
        Debug.Log($"Z 23 : {z_23}");
        Debug.Log($"Z Diff : {z_diff}");
        Debug.Log("#################################");

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
    enum SlopeType
    {
        YX,
        ZX,
        ZY
    }

    float GetSlopeBetweenLandmarks(int index1, int index2, float[] landmarks, SlopeType slopeType)
    {
        Dictionary<string, float> landmarkData1 = GetLandmarkData(index1, landmarks);
        Dictionary<string, float> landmarkData2 = GetLandmarkData(index2, landmarks);

        if (landmarkData1 != null && landmarkData2 != null)
        {
            float x1 = landmarkData1["x"];
            float y1 = landmarkData1["y"];
            float z1 = landmarkData1["z"];

            float x2 = landmarkData2["x"];
            float y2 = landmarkData2["y"];
            float z2 = landmarkData2["z"];

            // Calculate slope between the two landmarks (in 3D space)
            float deltaX = x2 - x1;
            float deltaY = y2 - y1;
            float deltaZ = z2 - z1;

            // Check for division by zero (to avoid potential runtime errors)
            if (deltaX != 0)
            {
                float slopeYX = deltaY / deltaX;
                float slopeZX = deltaZ / deltaX;
                float slopeZY = deltaZ / deltaY;
                
                // Return the requested slope based on slopeType
                switch (slopeType)
                {
                    case SlopeType.YX: return slopeYX;
                    case SlopeType.ZX: return slopeZX;
                    case SlopeType.ZY: return slopeZY;
                    default: return slopeZY;
                }
                // return slopeType == SlopeType.YZ ? slopeYZ : slopeXZ;
            }
            else
            {
                Console.WriteLine("Slope calculation error: Division by zero.");
                return float.NaN; // Return NaN (Not a Number) to indicate an error
            }
        }
        else
        {
            Console.WriteLine("Invalid landmark indices.");
            return float.NaN; // Return NaN (Not a Number) to indicate an error
        }
    }

    void RotateTable(float hip_slope, float vertical_slope){
        // transform.rotation = Quaternion.Euler(hip_slope, 0, vertical_slope); // Rotate the object in local space
        // transform.rotation = Quaternion.Euler(hip_slope * 10, 0, vertical_slope * 10); // Rotate the object in local space
        transform.rotation = Quaternion.Euler(hip_slope * 10, 0, vertical_slope); // Rotate the object in local space
    }

    void DoWork()
    {
        while (true)
        {
            // Receive data from client
            IPEndPoint clientEndpoint = null;
            byte[] data = server.Receive(ref clientEndpoint);

            Buffer.BlockCopy(data, 0, landmarks, 0, data.Length);

            // Debug.Log(GetLandmarkData(0, result)["x"]);
            hip_slope = GetSlopeBetweenLandmarks(23, 24, landmarks, SlopeType.YX);
            // Debug.Log($"Hip Slope between landmarks 23 and 25 (YX): {hip_slope}");
            vertical_slope = GetSlopeBetweenLandmarks(23, 11, landmarks, SlopeType.ZX);
            // Debug.Log($"Vertical Slope Slope between landmarks 23 and 11 (ZY): {vertical_slope}");

            float medianX_Low = (GetLandmarkData(23, landmarks)["x"] +GetLandmarkData(24, landmarks)["x"]) / 2f;
            float medianX_High = (GetLandmarkData(11, landmarks)["x"] +GetLandmarkData(12, landmarks)["x"]) / 2f;

            float medianZ_Low = (GetLandmarkData(23, landmarks)["z"] +GetLandmarkData(24, landmarks)["z"]) / 2f;
            float medianZ_High = (GetLandmarkData(11, landmarks)["z"] +GetLandmarkData(12, landmarks)["z"]) / 2f;

            slopeZX_median = (medianZ_High-medianZ_Low) / (medianX_High-medianX_Low);

            z_11 = GetLandmarkData(11, landmarks)["z"] ;

            z_23 = GetLandmarkData(23, landmarks)["z"];

            z_diff = GetLandmarkData(11, landmarks)["z"] - GetLandmarkData(23, landmarks)["z"];

            
            // Debug.Log(result[0]);
            // Debug.Log("Vertical Slope : ");
            // Debug.Log(result[1]);
            // print results length
            // Debug.Log(result.Length);
            // print results shape
            // Debug.Log("#################################");

            // Rotate the object based on the slopes
            // float rotationX = hip_slope * rotationSpeed; // Adjust the rotation speed as needed
            // float rotationZ = vertical_slope * rotationSpeed; // Adjust the rotation speed as needed

            // transform.Rotate(rotationX, 0, rotationZ); // Rotate the object in local space


        }

    }
}

