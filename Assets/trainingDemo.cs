using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDemo : MonoBehaviour
{
    private ProcessStartInfo startInfo;
    private Thread pythonThread;
    private bool isAcquiring = false;
    private string stateFilePath;

    // GameObjects for each label
    public GameObject RightObject;
    public GameObject LeftObject;
    public GameObject UpObject;
    public GameObject DownObject;

    private Dictionary<string, GameObject> labelToGameObject;

    void Start()
    {
        // Initialize the ProcessStartInfo with the path to your python executable and script
        startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";  // Use the full path to the python executable if necessary
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        stateFilePath = Path.Combine(Application.dataPath, "unicorn_state.json");

        // Initialize the dictionary with labels and corresponding GameObjects
        labelToGameObject = new Dictionary<string, GameObject>
        {
            { "R", RightObject },
            { "L", LeftObject },
            { "U", UpObject },
            { "D", DownObject }
        };

        // Log the initial state of GameObjects and set them inactive
        foreach (var kvp in labelToGameObject)
        {
            if (kvp.Value != null)
            {
                UnityEngine.Debug.Log($"{kvp.Key} object found: {kvp.Value.name}");
                kvp.Value.SetActive(false); // Hide all GameObjects initially
            }
            else
            {
                UnityEngine.Debug.LogError($"{kvp.Key} object not found in the scene!");
            }
        }

        // Start checking for label updates
        StartCoroutine(CheckForLabelUpdates());
    }

    void RunPythonScript(string script, string args, Action<string> outputCallback = null)
    {
        startInfo.Arguments = string.Format("\"{0}\" {1}", script, args);

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.Log("Output: " + e.Data);
                    outputCallback?.Invoke(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.LogError("Error: " + e.Data);
                    outputCallback?.Invoke(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }

    void RunPythonScriptAsync(string script, string args)
    {
        Thread thread = new Thread(() =>
        {
            RunPythonScript(script, args);
        });
        thread.Start();
    }

    void StartDataAcquisition()
    {
        if (!isAcquiring)
        {
            isAcquiring = true;
            string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
            pythonThread = new Thread(() => RunPythonScript(scriptPath, "start"));
            pythonThread.Start();
        }
    }

    void StopDataAcquisition()
    {
        if (isAcquiring)
        {
            isAcquiring = false;
            string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
            UnityEngine.Debug.Log("Stopping acquisition using script at: " + scriptPath);
            RunPythonScriptAsync(scriptPath, "stop");
            if (pythonThread != null && pythonThread.IsAlive)
            {
                pythonThread.Join();
            }
        }
    }

    public void ConnectDevice()
    {
        string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
        UnityEngine.Debug.Log("Connecting device using script at: " + scriptPath);
        RunPythonScript(scriptPath, "connect");
    }

    public void StartAcquisition()
    {
        StartDataAcquisition();
    }

    public void StopAcquisition()
    {
        StopDataAcquisition();
    }

    public void CloseConnection()
    {
        string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
        UnityEngine.Debug.Log("Closing connection using script at: " + scriptPath);
        RunPythonScript(scriptPath, "close");
    }

    void ListAvailableDevices()
    {
        string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
        UnityEngine.Debug.Log($"ListAvailableDevices: Listing available devices using script at: {scriptPath}");
        RunPythonScript(scriptPath, "list_devices", ProcessDeviceList);
    }

    void ProcessDeviceList(string jsonOutput)
    {
        UnityEngine.Debug.Log($"ProcessDeviceList: Processing device list: {jsonOutput}");

        // Assuming jsonOutput is a JSON array of device serials
        var devices = JsonUtility.FromJson<string[]>(jsonOutput);
        if (devices.Length > 0)
        {
            UnityEngine.Debug.Log("Available devices:");
            foreach (var device in devices)
            {
                UnityEngine.Debug.Log(device);
            }
        }
        else
        {
            UnityEngine.Debug.Log("No devices found.");
        }
    }

    public void LoadClassifier()
    {
        string scriptPath = Path.Combine(Application.dataPath, "trainedTesting.py");
        UnityEngine.Debug.Log($"LoadClassifier: Loading classifier using script at: {scriptPath}");
        RunPythonScript(scriptPath, "load_classifier ssvep_classifier.pkl");
    }

    void OnApplicationQuit()
    {
        StopDataAcquisition();
        CloseConnection();
    }

    private IEnumerator CheckForLabelUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Check every second

            if (File.Exists(stateFilePath))
            {
                string jsonContent = File.ReadAllText(stateFilePath);
                var state = JsonUtility.FromJson<Dictionary<string, string>>(jsonContent);

                if (state.ContainsKey("recent_label"))
                {
                    string recentLabel = state["recent_label"];
                    UnityEngine.Debug.Log($"Recent label: {recentLabel}");
                    UpdateGameObjects(recentLabel);
                }
                else
                {
                    UnityEngine.Debug.Log("No recent label found in state.");
                }
            }
            else
            {
                UnityEngine.Debug.Log("State file not found.");
            }
        }
    }

    private void UpdateGameObjects(string recentLabel)
    {
        // Hide all GameObjects initially
        foreach (var obj in labelToGameObject.Values)
        {
            if (obj != null) 
            {
                obj.SetActive(false);
            }
        }

        // Activate the GameObject corresponding to the recent label
        if (labelToGameObject.ContainsKey(recentLabel))
        {
            UnityEngine.Debug.Log($"Activating object for label: {recentLabel}");
            if (labelToGameObject[recentLabel] != null)
            {
                labelToGameObject[recentLabel].SetActive(true);
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"Label {recentLabel} does not have a corresponding GameObject.");
        }
    }
}
