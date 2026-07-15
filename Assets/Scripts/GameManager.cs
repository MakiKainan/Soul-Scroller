using System;
using System.Globalization;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double Money { get; private set; }
    public double MoneyPerScroll { get; private set; } = 1.0;

    public event Action<double> OnMoneyChanged;
    public event Action<double> OnMultiplierChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    public void AddMoney(double amount)
    {
        Money += amount;
        SaveData();
        OnMoneyChanged?.Invoke(Money);
    }

    public void BuyUpgrade(double amount)
    {
        if (Money < amount) return; // Guard: prevent negative money
        Money -= amount;
        SaveData();
        OnMoneyChanged?.Invoke(Money);
    }

    public void IncreaseMoneyPerScroll(double amount)
    {
        MoneyPerScroll += amount;
        SaveData();
        OnMultiplierChanged?.Invoke(MoneyPerScroll);
    }

    public void ResetMoney()
    {
        Money = 0;
        SaveData();
        OnMoneyChanged?.Invoke(Money);
    }

    private void SaveData()
    {
        // Use InvariantCulture to prevent locale-dependent parsing issues
        PlayerPrefs.SetString("Save_Money", Money.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetString("Save_MoneyPerScroll", MoneyPerScroll.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        string savedMoney = PlayerPrefs.GetString("Save_Money", "0");
        if (double.TryParse(savedMoney, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedMoney))
        {
            Money = parsedMoney;
        }

        string savedMPS = PlayerPrefs.GetString("Save_MoneyPerScroll", "1");
        if (double.TryParse(savedMPS, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedMPS))
        {
            MoneyPerScroll = parsedMPS;
        }
    }
}
