using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public GameObject helpText;
    PlayerShoot playerShoot;

    public void SetPlayer(GameObject player)
    {
        playerShoot = player.GetComponent<PlayerShoot>();
    }

    void Update()
    {
        StartCoroutine(FalseActive());
        if (playerShoot != null)
        {
            ammoText.text = "Ammo: " + playerShoot.shootsAvailable;
        }
    }

    private IEnumerator FalseActive()
    {
        yield return new WaitForSeconds(6f);
        helpText.SetActive(false);
    }
    
}
