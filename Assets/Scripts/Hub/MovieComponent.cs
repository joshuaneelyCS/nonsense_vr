
using UnityEngine;


public class MovieComponent : MonoBehaviour
{
    public MovieHubPoster myMovie;

    public void ShowConfirmationScreen()
    {
        // Show and send data to Confirmation Screen
       confirmationScreenHandler confirmationScreen = FindObjectOfType<confirmationScreenHandler>();
       confirmationScreen.Show(myMovie.title, myMovie.summary, myMovie.renderedPosterTexture, myMovie.cursor);
    }
}
