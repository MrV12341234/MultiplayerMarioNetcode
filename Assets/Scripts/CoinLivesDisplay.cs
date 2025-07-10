using TMPro;
using UnityEngine;

public class CoinLivesDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText; 
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private string coinPrefix = "× ";          // so it shows “ × 12”
    [SerializeField] private string livesPrefix = "Lives= ";
    

    private int lastShownCoins = -1;                             // ensure first update happens
    private int lastShownLives = -1;

    private void Update()
    {
        // defensively check the singleton exists
        if (GameManager.Instance == null) return;

        int currentCoins = GameManager.Instance.coins;
        int currentLives = GameManager.Instance.lives;

        // update only when the value changes
        if (currentCoins != lastShownCoins)
        {
            coinText.text = coinPrefix + currentCoins;
            lastShownCoins = currentCoins;
        }

        if (currentLives != lastShownLives)
        {
            livesText.text = livesPrefix + currentLives;
            lastShownLives = currentLives;

        }
    }
}
