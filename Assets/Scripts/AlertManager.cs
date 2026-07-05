using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private float fadeSpeed = 5f;

    private CanvasGroup canvasGroup;
    private Coroutine alertCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void ShowAlert(string message)
    {
        if (alertText != null)
        {
            alertText.text = message;
        }

        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
        }

        alertCoroutine = StartCoroutine(AnimateAlert());
    }

    private IEnumerator AnimateAlert()
    {
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(1f);

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}