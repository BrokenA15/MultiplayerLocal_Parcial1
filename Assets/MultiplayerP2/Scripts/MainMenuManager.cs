using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena de juego en Build Settings")]
    [SerializeField] private string nombreEscenaJuego = "P2Multi";

  
    public void Jugar()
    {
      
        
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    
    public void SalirDelJuego()
    {
        

       
        Application.Quit();

        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}