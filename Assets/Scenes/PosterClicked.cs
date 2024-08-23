using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PosterClicked : MonoBehaviour, IPointerClickHandler
{
    public MovieComponent movieComponent;
    public void OnPointerClick(PointerEventData eventData)
    {
        movieComponent.ShowConfirmationScreen();
    }
}
