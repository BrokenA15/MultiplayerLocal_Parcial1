using UnityEngine;

public class UIMORIR : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player != null && player.gameEnded)
        {
            gameObject.SetActive(false);
        }
    }
}
