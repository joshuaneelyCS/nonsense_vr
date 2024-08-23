using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class interactionWithPanels : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float initialHeight = 1320;
    public float targetHeight = 1320;
    public float duration = 0.1f;

    /* DESCRIPTION: This script handles when the user hovers over a panel that contains information that's to big to fit,
     * it grows to the size of the content, and then shrinks back to its original size when the user leaves the content
     * 
     * USES: 3rd Yellow Description Panel */
    
    // When the pointer enters the boxed area, grow the object to the target height
    public void OnPointerEnter(PointerEventData eventData)
    { 
        StartCoroutine(AnimateHeight(targetHeight, duration));
    }

    // When the pointer leaves the boxed area, shrink the object to its original height
    public void OnPointerExit(PointerEventData eventData)
    {

        StartCoroutine(AnimateHeight(initialHeight, duration));
        
    }
     // Reference to the RectTransform you want to animate
     // Duration of the animation

    private void Start()
    {
    }

    private IEnumerator AnimateHeight(float targetHeight, float duration)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        float elapsedTime = 0f;
        Vector2 initialSizeDelta = rectTransform.sizeDelta;
        Vector2 initialPosition = rectTransform.anchoredPosition;

        // Calculate the difference in height
        float heightDifference = targetHeight - initialSizeDelta.y;


        GameObject viewPort = GameObject.Find("DUI View Port");
        RectTransform viewPortRect = viewPort.GetComponent<RectTransform>();
        Vector2 initialViewSizeDelta = viewPortRect.sizeDelta;

        while (elapsedTime < duration)
        {
            // This is the ratio of height that should be done in the amount of time
            float t = elapsedTime / duration;

            // Interpolate sizeDelta and anchoredPosition
            rectTransform.sizeDelta = Vector2.Lerp(initialSizeDelta, new Vector2(initialSizeDelta.x, targetHeight), t);
            rectTransform.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y - heightDifference * (t * 0.5f));

            // This takes the original dimensions and the desired dimentions and smoothly changes t percent of that change
            viewPortRect.offsetMax = Vector2.Lerp(initialViewSizeDelta, new Vector2(initialViewSizeDelta.x, targetHeight), t);
            viewPortRect.offsetMin = new Vector2(0, 0); // Bottom-left // Top-Right
            viewPortRect.anchorMin = new Vector2(0, 1);
            viewPortRect.anchorMax = new Vector2(1, 1);
            viewPortRect.pivot = new Vector2(0, 1);
            viewPortRect.anchoredPosition3D = new Vector3(0, 0, 0);
            viewPortRect.localScale = new Vector3(1, 1, 1);

        elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final state is set
        rectTransform.sizeDelta = new Vector2(initialSizeDelta.x, targetHeight);
        rectTransform.anchoredPosition = new Vector2(
            initialPosition.x,
            initialPosition.y - heightDifference * 0.5f
        );
    }


    // Update is called once per frame
    void Update()
    {

    }
}
