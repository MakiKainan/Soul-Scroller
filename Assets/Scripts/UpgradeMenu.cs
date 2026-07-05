using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    public void BuyItems()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.Money >= 10.0)
            {
                GameManager.Instance.BuyUpgrade(10.0);
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
