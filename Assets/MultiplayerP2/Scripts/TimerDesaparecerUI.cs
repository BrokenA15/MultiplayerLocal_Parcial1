using UnityEngine;
using System.Collections;

public class TimerDesaparecerUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float tiempoDeEspera = 15f;
    [SerializeField] private GameObject objetoUI; // El panel o texto que quieres ocultar

    void Start()
    {
        // Si no asignaste nada en el inspector, intentamos usar este mismo objeto
        if (objetoUI == null)
        {
            objetoUI = this.gameObject;
        }

        // Iniciamos la cuenta regresiva
        StartCoroutine(EsconderDespuesDeTiempo());
    }

    private IEnumerator EsconderDespuesDeTiempo()
    {
        // Espera los segundos indicados
        yield return new WaitForSeconds(tiempoDeEspera);

        // Desactiva el objeto
        if (objetoUI != null)
        {
            objetoUI.SetActive(false);
            Debug.Log($"UI {objetoUI.name} ocultada después de {tiempoDeEspera} segundos.");
        }
    }

    // Método público por si quieres reiniciar el contador manualmente desde otro script
    public void ReiniciarContador()
    {
        StopAllCoroutines();
        objetoUI.SetActive(true);
        StartCoroutine(EsconderDespuesDeTiempo());
    }
}