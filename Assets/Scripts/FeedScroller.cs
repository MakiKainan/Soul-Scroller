using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class FeedScroller : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private float cardHeight = 1920f;
    [SerializeField] private double moneyPerSwipe = 1.0;
    [SerializeField] private float snapSpeed = 10f;
    // ponytail: tuned to half of the default cardHeight (1920) so a swipe
    // commits once you're "at the middle" — bump this together with cardHeight
    // if that value changes, not worth a derived formula for one scene.
    [SerializeField] private float swipeThreshold = 960f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int settledIndex;
    private int targetIndex;
    private bool isSnapping;
    private float dragStartAnchoredY;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isSnapping = false;
        dragStartAnchoredY = content.anchoredPosition.y;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = content.anchoredPosition.y - dragStartAnchoredY;

        // Top-anchored content: anchoredPosition.y increases as you scroll down to later cards.
        // Swipe up (reveal next card) = distance goes positive.
        if (dragDistance > swipeThreshold)
        {
            targetIndex = Mathf.Clamp(settledIndex + 1, 0, content.childCount - 1);
        }
        // Swipe down (reveal previous card) = distance goes negative.
        else if (dragDistance < -swipeThreshold)
        {
            targetIndex = Mathf.Clamp(settledIndex - 1, 0, content.childCount - 1);
        }
        else
        {
            // Weak/cancelled drag - snap to whichever card is nearest right now.
            targetIndex = CalculateNearestIndex(content.anchoredPosition.y, cardHeight, content.childCount);
        }

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
                GameManager.Instance.AddMoney(moneyPerSwipe);
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
