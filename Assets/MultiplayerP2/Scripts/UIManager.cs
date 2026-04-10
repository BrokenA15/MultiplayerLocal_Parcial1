using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject victoryPanel;
    public GameObject losePanel;

    void Awake()
    {
        Instance = this;
        victoryPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
    }

    public void ShowDefeat()
    {
        losePanel.SetActive(true);
    }
}