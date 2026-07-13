using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonText;

    private void Start()
    {
        // Pastikan Manager sudah ada, jika belum kita buat
        if (DailyQuestManager.Instance == null)
        {
            GameObject managerObj = new GameObject("DailyQuestManager");
            managerObj.AddComponent<DailyQuestManager>();
        }

        DailyQuestManager.Instance.OnQuestProgressChanged += UpdateUI;
        claimButton.onClick.AddListener(OnClaimClicked);

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnClaimClicked);
        }

        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestProgressChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        var manager = DailyQuestManager.Instance;

        questDescriptionText.text = $"Scroll {manager.SwipeGoal} times today.\nReward: ${manager.RewardAmount}";
        progressText.text = $"Progress: {manager.SwipesToday} / {manager.SwipeGoal}";

        if (manager.IsRewardClaimed)
        {
            claimButton.interactable = false;
            claimButtonText.text = "Claimed!";
        }
        else if (manager.SwipesToday >= manager.SwipeGoal)
        {
            claimButton.interactable = true;
            claimButtonText.text = "Claim Reward";
        }
        else
        {
            claimButton.interactable = false;
            claimButtonText.text = "Incomplete";
        }
    }

    private void OnClaimClicked()
    {
        DailyQuestManager.Instance.ClaimReward();
    }
}
