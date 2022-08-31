using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    public void StartARSession()
    {
        SceneManager.LoadScene("ARMuseumProject_MainScene");
    }

    public void StartHelpScene()
    {
        SceneManager.LoadScene("ARMuseumProject_Help");
    }
}
