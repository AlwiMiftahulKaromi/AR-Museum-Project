using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartARSession()
    {
        SceneManager.LoadScene("ARMuseumProject_MainScene");
    }
}
