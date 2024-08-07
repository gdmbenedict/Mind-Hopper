using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using UnityEngine;

public class UnicornPython : MonoBehaviour
{
    private ProcessStartInfo startInfo;
    private Thread pythonThread;
    private bool isAcquiring = false;

    void Start()
    {
        // Initialize the ProcessStartInfo with the path to your python executable and script
        startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";  // Use the full path to the python executable if necessary
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
    }

    void RunPythonScript(string script, string args)
    {
        startInfo.Arguments = string.Format("\"{0}\" {1}", script, args);

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.Log("Output: " + e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.LogError("Error: " + e.Data);
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
            string scriptPath = Path.Combine(Application.dataPath, "UnicornCPython.py");
            pythonThread = new Thread(() => RunPythonScript(scriptPath, "start"));
            pythonThread.Start();
        }
    }

    void StopDataAcquisition()
    {
        if (isAcquiring)
        {
            isAcquiring = false;
            string scriptPath = Path.Combine(Application.dataPath, "UnicornCPython.py");
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
        string scriptPath = Path.Combine(Application.dataPath, "UnicornCPython.py");
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
        string scriptPath = Path.Combine(Application.dataPath, "UnicornCPython.py");
        UnityEngine.Debug.Log("Closing connection using script at: " + scriptPath);
        RunPythonScript(scriptPath, "close");
    }

    void OnApplicationQuit()
    {
        StopDataAcquisition();
        CloseConnection();
    }
}
