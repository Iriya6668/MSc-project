using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FPSDataRecorder : MonoBehaviour
{
    public float updateInterval = 0.5f; // 
    private float deltaTime = 0.0f;
    private float timeSinceLastUpdate = 0.0f;

    private List<float> fpsData = new List<float>(); // 

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            float fps = 1.0f / deltaTime;
            fpsData.Add(fps);
            timeSinceLastUpdate = 0.0f;
        }
    }

    void OnApplicationQuit()
    {
        SaveFPSDataToDesktop();
    }

    void SaveFPSDataToDesktop()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "FPSData.csv");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Time (s), FPS");
            for (int i = 0; i < fpsData.Count; i++)
            {
                float time = i * updateInterval;
                writer.WriteLine($"{time}, {fpsData[i]}");
            }
        }
        Debug.Log("FPS data saved to " + filePath);
    }
}
