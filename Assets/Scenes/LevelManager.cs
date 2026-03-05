using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    //[SerializedField] public string sceneToLoad;
    public void LevelLoader( string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
