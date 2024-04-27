using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Correct namespace for UI components like Image

public class ImageFlicker : MonoBehaviour
{
    public Image targetImage; // assign to the inspector
    public float flickerfrequency = 12.0f; // frequency in seconds

    // Start is called before the first frame update
    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        StartCoroutine(FlickerRoutine());
    }

    // Coroutine to flicker the image
    private IEnumerator FlickerRoutine()
    {
        while(true)
        {
            targetImage.enabled = !targetImage.enabled;
            yield return new WaitForSeconds(1/flickerfrequency);
        }
    }
}
