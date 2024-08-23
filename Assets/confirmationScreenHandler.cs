using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class confirmationScreenHandler : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI summaryText;
    public RawImage posterImage;
    public void Show(string title, string summary, Texture posterImageTexture, string cursor)
    {
        
        Transform childTransform = transform.GetChild(0);

        titleText.text = title;
        summaryText.text = summary;
        posterImage.texture = posterImageTexture;

        // This is a variable that is passed onto the next scene to display in the theater
        SceneDataToSend.SceneCursor = cursor;
        childTransform.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Transform childTransform = transform.GetChild(0);
        childTransform.gameObject.SetActive(false);
    }

}
