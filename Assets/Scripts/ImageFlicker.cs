using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Correct namespace for UI components like Image

public class ImageFlicker : MonoBehaviour
{
    public SpriteRenderer targetImage; // Assign in the Inspector
    public float flickerFrequency = 12.0f; // Frequency in seconds
    private bool isFlickering = false; // Control the flickering

    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<SpriteRenderer>();
        }
    }

    // Method to start flickering
    public void StartFlickering()
    {
        
        if (!isFlickering)
        {
            Debug.Log("reached coroutine start");
            isFlickering = true;
            StartCoroutine(FlickerRoutine());   
        }
    }

    // Method to stop flickering
    public void StopFlickering()
    {
        isFlickering = false;
        StopCoroutine(FlickerRoutine());       
        targetImage.enabled = true; // Optionally, ensure the image is visible when stopping
    }

    private IEnumerator FlickerRoutine()
    {
        while (isFlickering)
        {
            targetImage.enabled = !targetImage.enabled;
            yield return new WaitForSeconds(1/flickerFrequency);
        }
    }
}
