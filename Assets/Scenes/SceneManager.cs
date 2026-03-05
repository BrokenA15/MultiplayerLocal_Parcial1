using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public string abc;

public class SceneManager : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene(abc);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
