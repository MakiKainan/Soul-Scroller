using TMPro;
using UnityEngine;

public class MoneyLabel : MonoBehaviour
{
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.OnMoneyChanged += UpdateLabel;
        UpdateLabel(GameManager.Instance.Money);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateLabel;
        }
    }

    private void UpdateLabel(double money)
    {
        label.text = $"${money:0}";
    }
}
