using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void ToUpgradeShop()
    {
        SceneManager.LoadScene("UpgradeShop");
    }

    public void BackToGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void CallResetMoneyManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetMoney();
        }
    }
}
