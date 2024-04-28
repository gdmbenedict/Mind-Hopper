using UnityEngine;

public class flicker_manager : MonoBehaviour
{
    public ImageFlicker leftFlicker;
    public ImageFlicker rightFlicker;
    public ImageFlicker upFlicker;
    public ImageFlicker downFlicker;

    public void StartFlickering()
    {
        if (leftFlicker != null)
        {
            leftFlicker.StartFlickering();
        }
        else
        {
            Debug.LogError("LeftFlicker is not assigned in flicker_manager.");
        }

        if (rightFlicker != null)
        {
            rightFlicker.StartFlickering();
        }
        else
        {
            Debug.LogError("RightFlicker is not assigned in flicker_manager.");
        }

        if (upFlicker != null)
        {
            upFlicker.StartFlickering();
        }
        else
        {
            Debug.LogError("UpFlicker is not assigned in flicker_manager.");
        }

        if (downFlicker != null)
        {
            downFlicker.StartFlickering();
        }
        else
        {
            Debug.LogError("DownFlicker is not assigned in flicker_manager.");
        }
    }
}


