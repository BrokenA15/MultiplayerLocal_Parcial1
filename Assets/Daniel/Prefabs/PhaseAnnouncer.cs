using UnityEngine;
using TMPro; // <--- ESTA ES LA LÍNEA QUE FALTA
using Unity.Netcode;

public class PhaseAnnouncer : MonoBehaviour
{
    public TextMeshProUGUI textoFase;

    void Start()
    {
        // Verificamos que el TurnManager exista antes de suscribirnos
        if (TurnManager1.Instance != null)
        {
            // Nos suscribimos al cambio de fase
            TurnManager1.Instance.currentPhase.OnValueChanged += AlCambiarFase;

            // Seteamos el texto inicial
            ActualizarTexto(TurnManager1.Instance.currentPhase.Value);
        }
    }

    void Update()
    {
        // Si todavía no estamos suscritos y el manager ya apareció...
        if (TurnManager1.Instance != null && !suscrito)
        {
            TurnManager1.Instance.currentPhase.OnValueChanged += AlCambiarFase;
            ActualizarTexto(TurnManager1.Instance.currentPhase.Value);
            suscrito = true;
        }
    }
    private bool suscrito = false;

    private void AlCambiarFase(TurnManager1.GamePhase valorAnterior, TurnManager1.GamePhase valorNuevo)
    {
        ActualizarTexto(valorNuevo);
    }

    private void ActualizarTexto(TurnManager1.GamePhase fase)
    {
        Debug.Log("Intentando cambiar texto a: " + fase.ToString()); // LOG 1
        if (textoFase != null)
        {
            textoFase.text = "FASE: " + fase.ToString().ToUpper();
            Debug.Log("¡Texto cambiado con éxito!"); // LOG 2
        }
        else
        {
            Debug.LogError("¡ERROR: No has asignado el componente de texto en el Inspector!");
        }
    }

    // Es buena práctica desuscribirse cuando el objeto se destruye
    void OnDestroy()
    {
        if (TurnManager1.Instance != null)
        {
            TurnManager1.Instance.currentPhase.OnValueChanged -= AlCambiarFase;
        }
    }
}