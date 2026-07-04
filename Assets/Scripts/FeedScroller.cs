using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class FeedScroller : MonoBehaviour, IEndDragHandler
{
    [SerializeField] private float cardHeight = 1920f;
    [SerializeField] private double moneyPerSwipe = 1.0;
    [SerializeField] private float snapSpeed = 10f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int settledIndex;
    private int targetIndex;
    private bool isSnapping;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        targetIndex = CalculateNearestIndex(content.anchoredPosition.y, cardHeight, content.childCount);
        isSnapping = true;
    }

    private void Update()
    {
        if (!isSnapping) return;

        float targetY = -targetIndex * cardHeight;
        Vector2 pos = content.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, snapSpeed * Time.deltaTime);

        if (Mathf.Abs(pos.y - targetY) < 0.5f)
        {
            pos.y = targetY;
            isSnapping = false;

            if (targetIndex != settledIndex)
            {
                settledIndex = targetIndex;
                GameManager.Instance.AddMoney(moneyPerSwipe);
            }
        }

        content.anchoredPosition = pos;
    }

    public static int CalculateNearestIndex(float contentAnchoredY, float cardHeight, int cardCount)
    {
        if (cardCount <= 0) return 0;
        int raw = Mathf.RoundToInt(-contentAnchoredY / cardHeight);
        return Mathf.Clamp(raw, 0, cardCount - 1);
    }
}
