using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] private double cost = 10.0;
    [SerializeField] private double moneyPerScrollIncrease = 1.0;

    public void BuyItems()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.Money >= cost)
            {
                GameManager.Instance.BuyUpgrade(cost);
                GameManager.Instance.IncreaseMoneyPerScroll(moneyPerScrollIncrease);
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
}
