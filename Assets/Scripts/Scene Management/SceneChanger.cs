using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Call this method to change the scene
    public void ChangeScene(string sceneName)
    {

        SceneManager.LoadScene(sceneName);
    }

}

public static class SceneDataToSend
{
    public static string SceneCursor { get; set; }
}
