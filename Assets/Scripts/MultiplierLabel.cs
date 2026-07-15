using TMPro;
using UnityEngine;

public class MultiplierLabel : MonoBehaviour
{
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.OnMultiplierChanged += UpdateLabel;
        UpdateLabel(GameManager.Instance.MoneyPerScroll);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMultiplierChanged -= UpdateLabel;
        }
    }

    private void UpdateLabel(double multiplier)
    {
        if (multiplier <= 1.0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            label.text = $" (x{multiplier:0.#})";
        }
    }
}
