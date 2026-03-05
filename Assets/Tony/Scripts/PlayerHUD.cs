using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public GameObject helpText;
    public GameObject shootText;
    PlayerShoot playerShoot;
    PlayerMovement winLoseCondition;
    GameObject _player;
    public GameObject LoseScreen;
    public GameObject WinScreen;
    public void SetPlayer(GameObject player)
    {
        playerShoot = player.GetComponent<PlayerShoot>();
        winLoseCondition = player.GetComponent<PlayerMovement>();
        _player = player;
    }

    void Start()
    {
        StartCoroutine(FalseActive());
        WinScreen.SetActive(false);
        LoseScreen.SetActive(false);
    }
    void Update()
    {
        
        if (playerShoot != null)
        {
            ammoText.text = "Ammo: " + playerShoot.shootsAvailable;
        }
        if (winLoseCondition.gameOver == true)
        {
            LoseScreen.SetActive(true);
        }

        if (winLoseCondition.gameWin == true)
        {
            WinScreen.SetActive(true);
            
           
        }
    }

    private IEnumerator FalseActive()
    {
        yield return new WaitForSeconds(7f);
        helpText.SetActive(false);
        shootText.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
