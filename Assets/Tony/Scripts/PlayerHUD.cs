using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    PlayerShoot playerShoot;

    public void SetPlayer(GameObject player)
    {
        playerShoot = player.GetComponent<PlayerShoot>();
    }

    void Update()
    {
        if (playerShoot != null)
        {
            ammoText.text = "Ammo: " + playerShoot.shootsAvailable;
        }
    }
}
