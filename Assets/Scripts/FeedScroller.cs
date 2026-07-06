using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class FeedScroller : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private float cardHeight;

    [SerializeField] private float snapSpeed = 10f;

    [Header("Sensitivitas Tarikan")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float flickVelocityThreshold = 300f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int settledIndex;
    private int targetIndex;
    private bool isSnapping;
    private float dragStartAnchoredY;
    private float maxScrollUpLimit;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;

        RectTransform viewportRect = scrollRect.viewport != null ? scrollRect.viewport : GetComponent<RectTransform>();
        cardHeight = viewportRect.rect.height;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isSnapping = false;
        dragStartAnchoredY = content.anchoredPosition.y;

        maxScrollUpLimit = (settledIndex * cardHeight) - (cardHeight * 0.25f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = content.anchoredPosition;

        if (pos.y < maxScrollUpLimit)
        {
            pos.y = maxScrollUpLimit;
            content.anchoredPosition = pos;

            scrollRect.velocity = Vector2.zero;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = content.anchoredPosition.y - dragStartAnchoredY;
        float currentVelocityY = scrollRect.velocity.y;

        if (dragDistance > swipeThreshold || currentVelocityY > flickVelocityThreshold)
        {
            targetIndex = Mathf.Clamp(settledIndex + 1, 0, content.childCount - 1);
        }
        else
        {
            targetIndex = settledIndex;
        }

        scrollRect.velocity = Vector2.zero;
        isSnapping = true;
    }

    private void Update()
    {
        if (!isSnapping) return;

        float targetY = targetIndex * cardHeight;
        Vector2 pos = content.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, snapSpeed * Time.deltaTime);

        if (Mathf.Abs(pos.y - targetY) < 1f)
        {
            pos.y = targetY;
            isSnapping = false;

            if (targetIndex != settledIndex)
            {
                settledIndex = targetIndex;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddMoney(GameManager.Instance.MoneyPerScroll);
                }
                if (DailyQuestManager.Instance != null)
                {
                    DailyQuestManager.Instance.AddSwipe();
                }
            }
        }

        content.anchoredPosition = pos;
    }

    public static int CalculateNearestIndex(float contentAnchoredY, float cardHeight, int cardCount)
    {
        if (cardCount <= 0) return 0;
        int raw = Mathf.RoundToInt(contentAnchoredY / cardHeight);
        return Mathf.Clamp(raw, 0, cardCount - 1);
    }
}