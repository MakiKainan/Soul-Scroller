using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PanelSwipeIn : MonoBehaviour
{
    [SerializeField] private float offscreenOffsetX = 1080f;
    [SerializeField] private float duration = 0.35f;

    private void Start()
    {
        var rt = (RectTransform)transform;
        Vector2 restingPos = rt.anchoredPosition;
        rt.anchoredPosition = restingPos + new Vector2(offscreenOffsetX, 0f);
        StartCoroutine(SlideIn(rt, restingPos));
    }

    private IEnumerator SlideIn(RectTransform rt, Vector2 restingPos)
    {
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, restingPos, t);
            yield return null;
        }

        rt.anchoredPosition = restingPos;
    }
}
