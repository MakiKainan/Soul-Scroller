using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double Money { get; private set; }
    public double MoneyPerScroll { get; private set; } = 1.0;

    public event Action<double> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(double amount)
    {
        // ponytail: plain double is enough for phase 1 (no upgrades/exponential
        // growth yet). Swap to a BigDouble type when upgrades land, per CLAUDE.md.
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    public void BuyUpgrade(double amount)
    {
        // ponytail: plain double is enough for phase 1 (no upgrades/exponential
        // growth yet). Swap to a BigDouble type when upgrades land, per CLAUDE.md.
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
    }

    public void IncreaseMoneyPerScroll(double amount)
    {
        MoneyPerScroll += amount;
    }

    public void ResetMoney()
    {
        Money = 0;
        OnMoneyChanged?.Invoke(Money);
    }
}
