using System;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    public int SwipesToday { get; private set; }
    public bool IsRewardClaimed { get; private set; }

    // Quest Harian: Scroll sebanyak 50 kali
    public int SwipeGoal = 50;
    public double RewardAmount = 100.0;

    public event Action OnQuestProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
        CheckNewDay();
    }

    public void AddSwipe()
    {
        if (IsRewardClaimed) return; // Berhenti menghitung kalau sudah diklaim hari ini

        SwipesToday++;
        SaveProgress();
        OnQuestProgressChanged?.Invoke();
    }

    public void ClaimReward()
    {
        if (SwipesToday >= SwipeGoal && !IsRewardClaimed)
        {
            IsRewardClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(RewardAmount);
            }
            SaveProgress();
            OnQuestProgressChanged?.Invoke();
        }
    }

    private void CheckNewDay()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string lastLogin = PlayerPrefs.GetString("DailyQuest_LastLoginDate", "");

        if (today != lastLogin)
        {
            // Reset karena sudah beda hari
            SwipesToday = 0;
            IsRewardClaimed = false;
            PlayerPrefs.SetString("DailyQuest_LastLoginDate", today);
            SaveProgress();
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("DailyQuest_SwipesToday", SwipesToday);
        PlayerPrefs.SetInt("DailyQuest_IsRewardClaimed", IsRewardClaimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        SwipesToday = PlayerPrefs.GetInt("DailyQuest_SwipesToday", 0);
        IsRewardClaimed = PlayerPrefs.GetInt("DailyQuest_IsRewardClaimed", 0) == 1;
    }
}
