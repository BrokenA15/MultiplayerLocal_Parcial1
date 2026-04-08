using UnityEngine;
using Unity.Netcode;
using TMPro;

public class TurnManager : NetworkBehaviour
{
    public int turn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(IsOwner)
        {
            turn = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (turn == 0)
        {

        }
        else if (turn == 1)
        {

        }


    }
}
