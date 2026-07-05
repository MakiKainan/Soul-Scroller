using UnityEngine;
using UnityEngine.UI;

public class FeedSpawner : MonoBehaviour
{
    [SerializeField] private RectTransform cardPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private int cardCount = 50;

    private void Start()
    {
        // Cards must be exactly one viewport tall to match FeedScroller's snap math
        // (it snaps by viewport height, not by a fixed card size) — otherwise the
        // two drift apart over many swipes and snapping lands between cards.
        float viewportHeight = ((RectTransform)content.parent).rect.height;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = Instantiate(cardPrefab, content);
            card.GetComponent<Image>().color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
            card.GetComponent<LayoutElement>().preferredHeight = viewportHeight;
        }
    }
}
