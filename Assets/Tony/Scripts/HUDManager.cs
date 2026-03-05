using UnityEngine;

public class HUDManager : MonoBehaviour
{
     public static HUDManager instance;
    
        public RectTransform canvas;
        public GameObject hudPrefab;
    
        private int playerCount = 0;
    
        void Awake()
        {
            instance = this;
        }
    
        public void CreateHUD(GameObject player)
        {
            GameObject hud = Instantiate(hudPrefab, canvas);
            RectTransform rect = hud.GetComponent<RectTransform>();
    
            SetHUDPosition(rect, playerCount);
    
            PlayerHUD playerHUD = hud.GetComponent<PlayerHUD>();
            playerHUD.SetPlayer(player);

            playerCount++;
        }
    
        void SetHUDPosition(RectTransform hud, int index)
        {
            if (index == 0) // Player 1
            {
                hud.anchorMin = new Vector2(0f, 0.5f);
                hud.anchorMax = new Vector2(0.5f, 1f);
                hud.pivot = new Vector2(0f, 1f);
                hud.anchoredPosition = new Vector2(20f, -20f);
            }
            else if (index == 1) // Player 2
            {
                hud.anchorMin = new Vector2(0.5f, 0.5f);
                hud.anchorMax = new Vector2(1f, 1f);
                hud.pivot = new Vector2(1f, 1f);
                hud.anchoredPosition = new Vector2(-20f, -20f);
            }
            else if (index == 2) // Player 3
            {
                hud.anchorMin = new Vector2(0f, 0f);
                hud.anchorMax = new Vector2(0.5f, 0.5f);
                hud.pivot = new Vector2(0f, 0f);
                hud.anchoredPosition = new Vector2(20f, 20f);
            }
            else if (index == 3) // Player 4
            {
                hud.anchorMin = new Vector2(0.5f, 0f);
                hud.anchorMax = new Vector2(1f, 0.5f);
                hud.pivot = new Vector2(1f, 0f);
                hud.anchoredPosition = new Vector2(-20f, 20f);
            }

            hud.offsetMin = Vector2.zero;
            hud.offsetMax = Vector2.zero;
        }
}
