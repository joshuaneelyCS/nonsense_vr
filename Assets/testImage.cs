using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testImage : MonoBehaviour
{
    public LoadImageFromUrl loadImageScript; // Assign this in the Inspector

    void Start()
    {
        if (loadImageScript != null)
        {
            Debug.Log("Testing image");
            // StartCoroutine(loadImageScript.LoadImage());
        }
        else
        {
            Debug.LogError("LoadImageFromUrl script not assigned.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
