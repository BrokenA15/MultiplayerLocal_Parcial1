using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class TurnManager1 : NetworkBehaviour
{
    public static TurnManager1 Instance;

    public enum GamePhase { Construccion, Disparo }
    public NetworkVariable<ulong> currentTurn = new NetworkVariable<ulong>();
    public NetworkVariable<GamePhase> currentPhase = new NetworkVariable<GamePhase>(GamePhase.Construccion);

    public NetworkVariable<ulong> activeCharacterNetworkId = new NetworkVariable<ulong>();

    public NetworkVariable<bool> personajeComprometido = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private List<PlayerAction> todosLosPersonajes = new List<PlayerAction>();

    // 🔒 Límite de barreras por jugador (clientId -> cantidad usada)
    private Dictionary<ulong, int> barrerasUsadas = new Dictionary<ulong, int>();
    public const int MAX_BARRERAS = 5;

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;

        if (IsServer)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            if (clients.Count > 0) currentTurn.Value = clients[0];
        }

        activeCharacterNetworkId.OnValueChanged += AlCambiarPersonajeActivo;
    }

    public bool IsMyTurn(ulong clientId) => currentTurn.Value == clientId;

    [Rpc(SendTo.Server)]
    public void ComprometерPersonajeServerRpc(ulong clientId)
    {
        if (currentTurn.Value != clientId) return;
        if (personajeComprometido.Value) return;
        personajeComprometido.Value = true;
    }

    public void RegistrarPersonaje(PlayerAction personaje)
    {
        if (!IsServer) return;

        if (!todosLosPersonajes.Contains(personaje))
            todosLosPersonajes.Add(personaje);

        if (todosLosPersonajes.Count == 1)
        {
            currentTurn.Value = personaje.OwnerClientId;
            activeCharacterNetworkId.Value = personaje.NetworkObject.NetworkObjectId;
        }
    }

    // 🔑 Llamado desde PlayerController cuando un clon muere
    public void DesregistrarPersonaje(PlayerAction personaje)
    {
        if (!IsServer) return;

        todosLosPersonajes.Remove(personaje);

        // Si el personaje muerto era el activo, pasamos al siguiente del mismo equipo
        if (activeCharacterNetworkId.Value == personaje.NetworkObject.NetworkObjectId)
        {
            todosLosPersonajes.RemoveAll(p => p == null);

            List<PlayerAction> compañeros = todosLosPersonajes
                .Where(p => p.OwnerClientId == personaje.OwnerClientId)
                .ToList();

            if (compañeros.Count > 0)
            {
                // Quedan compañeros vivos — activar el primero
                activeCharacterNetworkId.Value = compañeros[0].NetworkObject.NetworkObjectId;
            }
            // Si no quedan compañeros, PlayerController.TakeDamage ya declarará fin del juego
        }
    }

    [Rpc(SendTo.Server)]
    public void EndTurnServerRpc()
    {
        if (currentPhase.Value == GamePhase.Disparo)
        {
            // Solo aquí cambia el turno — después de disparar (o pasar el disparo)
            PasarTurnoAlSiguienteEquipo();
            currentPhase.Value = GamePhase.Construccion;
        }
        else
        {
            // Construccion → Disparo siempre, aunque no haya construido nada
            currentPhase.Value = GamePhase.Disparo;
        }
    }

    // 🔒 Verifica si el jugador puede colocar más barreras
    public bool PuedeConstructor(ulong clientId)
    {
        if (!barrerasUsadas.ContainsKey(clientId)) return true;
        return barrerasUsadas[clientId] < MAX_BARRERAS;
    }

    // 🔒 Registra una barrera usada por el jugador
    public void RegistrarBarrera(ulong clientId)
    {
        if (!barrerasUsadas.ContainsKey(clientId))
            barrerasUsadas[clientId] = 0;
        barrerasUsadas[clientId]++;
    }

    private void PasarTurnoAlSiguienteEquipo()
    {
        if (todosLosPersonajes.Count == 0) return;

        todosLosPersonajes.RemoveAll(p => p == null);

        var listaClientes = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        int indiceJugadorActual = listaClientes.IndexOf(currentTurn.Value);
        ulong siguienteJugadorId = listaClientes[(indiceJugadorActual + 1) % listaClientes.Count];

        currentTurn.Value = siguienteJugadorId;
        personajeComprometido.Value = false;

        List<PlayerAction> personajesDelNuevoJugador = todosLosPersonajes
            .Where(p => p.OwnerClientId == siguienteJugadorId)
            .ToList();

        if (personajesDelNuevoJugador.Count > 0)
            activeCharacterNetworkId.Value = personajesDelNuevoJugador[0].NetworkObject.NetworkObjectId;
    }

    [Rpc(SendTo.Server)]
    public void CambiarPersonajePropioServerRpc(ulong clientId)
    {
        if (currentPhase.Value != GamePhase.Construccion) return;
        if (currentTurn.Value != clientId) return;
        if (personajeComprometido.Value) return;

        todosLosPersonajes.RemoveAll(p => p == null);

        List<PlayerAction> misPersonajes = todosLosPersonajes
            .Where(p => p.OwnerClientId == clientId)
            .ToList();

        if (misPersonajes.Count <= 1) return;

        int miIndiceActual = misPersonajes.FindIndex(p => p.NetworkObject.NetworkObjectId == activeCharacterNetworkId.Value);
        int miSiguienteIndice = (miIndiceActual + 1) % misPersonajes.Count;

        activeCharacterNetworkId.Value = misPersonajes[miSiguienteIndice].NetworkObject.NetworkObjectId;
    }

    private void AlCambiarPersonajeActivo(ulong idAnterior, ulong idNuevo)
    {
        ActualizarControlesLocalesRpc(idNuevo);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ActualizarControlesLocalesRpc(ulong idPersonajeActivo)
    {
        PlayerAction[] personajesEnEscena = FindObjectsByType<PlayerAction>(FindObjectsSortMode.None);

        foreach (PlayerAction p in personajesEnEscena)
        {
            bool esElActivo = (p.NetworkObject.NetworkObjectId == idPersonajeActivo);

            p.SetHighlight(esElActivo);

            p.enabled = esElActivo;

            if (p.TryGetComponent(out PlayerController movimiento))
                movimiento.enabled = esElActivo;

            if (p.TryGetComponent(out PlayerShooting disparo))
            {
                if (!esElActivo)
                {
                    // Personaje inactivo: apagar disparo siempre
                    disparo.enabled = false;
                }
                else
                {
                    // Personaje activo: PlayerAction.Update() maneja el encendido
                    // según la fase. Solo forzamos aquí si YA estamos en Disparo
                    // para no perder el turno si el RPC llega tarde.
                    if (currentPhase.Value == GamePhase.Disparo)
                        disparo.enabled = true;
                    // Si estamos en Construccion, PlayerAction.Update() lo apagará
                    // y lo volverá a encender cuando cambie la fase
                }
            }

            if (!esElActivo)
                p.LimpiarGhost();
        }

        // 📷 CÁMARA: Moverla al personaje activo (solo en este cliente)
        if (NetworkManager.Singleton == null) return;

        // Buscamos el Transform del personaje activo para dárselo a la cámara
        foreach (PlayerAction p in personajesEnEscena)
        {
            if (p.NetworkObject.NetworkObjectId == idPersonajeActivo)
            {
                // Solo seguimos con la cámara si este personaje nos pertenece
                // (cada cliente sigue a SU personaje activo del turno actual)
                // En worms la cámara sigue al personaje activo independientemente del dueño
                if (CameraManager.Instance != null)
                    CameraManager.Instance.FollowTarget(p.transform);
                break;
            }
        }
    }
}