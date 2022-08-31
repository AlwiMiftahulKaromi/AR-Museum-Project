using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("ARMuseumProject_MainMenu");
    }
    public void StartARSession()
    {
        SceneManager.LoadScene("ARMuseumProject_MainScene");
    }

    public void StartHelpScene()
    {
        SceneManager.LoadScene("ARMuseumProject_Help");
    }
}
