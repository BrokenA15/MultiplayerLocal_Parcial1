using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [SerializeField] private string nombreEscena1 = "P2Multi";
    [SerializeField] private string nombreEscena2 = "P2Multi2";
    [SerializeField] private string nombreEscena3 = "P2Multi3";


    public void Jugar()
    {
        string[] escenas = { nombreEscena1, nombreEscena2, nombreEscena3 };
        string escenaAleatoria = escenas[Random.Range(0, escenas.Length)];
        SceneManager.LoadScene(escenaAleatoria);
    }

    public void SalirDelJuego()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}