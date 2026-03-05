using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<PlayerMovement> players = new List<PlayerMovement>();

    void Awake()
    {
        instance = this;
    }

    public void RegisterPlayer(PlayerMovement player)
    {
        players.Add(player);
    }

    public void CheckWinCondition()
    {
        int alivePlayers = 0;
        PlayerMovement lastAlive = null;

        foreach (PlayerMovement p in players)
        {
            if (!p.gameOver)
            {
                alivePlayers++;
                lastAlive = p;
            }
        }

        if (alivePlayers == 1 && lastAlive != null)
        {
            StartCoroutine(WinDelay(lastAlive));
        }
    }

    IEnumerator WinDelay(PlayerMovement winner)
    {
        
        
        yield return new WaitForSeconds(2f);

        winner.gameWin = true;
       
    }
}
