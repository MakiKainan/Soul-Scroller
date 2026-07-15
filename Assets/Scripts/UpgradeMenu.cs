using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Base Settings")]
    [SerializeField] private double baseCost = 10.0;
    [SerializeField] private double moneyPerScrollIncrease = 1.0;

    [Header("Price Inflation")]
    [SerializeField] private double costMultiplier = 1.15;
    [SerializeField] private string upgradeSaveKey = "Upgrade_Level_1";

    private int currentLevel = 0;
    private double currentCost;

    private void Start()
    {
        LoadUpgradeData();
        CalculateCurrentCost();
        UpdateUI(); // <-- PANGGIL DI SINI: Biar teks muncul pas pertama kali game jalan
    }

    public void BuyItems()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.Money >= currentCost)
            {
                GameManager.Instance.BuyUpgrade(currentCost);
                GameManager.Instance.IncreaseMoneyPerScroll(moneyPerScrollIncrease);

                currentLevel++;
                SaveUpgradeData();
                CalculateCurrentCost();
                UpdateUI(); // <-- PANGGIL DI SINI: Biar teks update ke harga baru setelah beli
            }
            else
            {
                if (AlertManager.Instance != null)
                {
                    AlertManager.Instance.ShowAlert("Not enough money to buy items!");
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (costText != null)
        {
            costText.text = $"Buy ${currentCost:N0}";
        }
    }

    private void CalculateCurrentCost()
    {
        currentCost = baseCost * Mathf.Pow((float)costMultiplier, currentLevel);
        currentCost = System.Math.Round(currentCost);
    }

    private void SaveUpgradeData()
    {
        PlayerPrefs.SetInt(upgradeSaveKey, currentLevel);
        PlayerPrefs.Save();
    }

    private void LoadUpgradeData()
    {
        currentLevel = PlayerPrefs.GetInt(upgradeSaveKey, 0);
    }
}