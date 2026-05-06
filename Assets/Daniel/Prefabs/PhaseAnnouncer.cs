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

    private void AlCambiarFase(TurnManager1.GamePhase valorAnterior, TurnManager1.GamePhase valorNuevo)
    {
        ActualizarTexto(valorNuevo);
    }

    private void ActualizarTexto(TurnManager1.GamePhase fase)
    {
        if (textoFase != null)
        {
            textoFase.text = "FASE: " + fase.ToString().ToUpper();
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