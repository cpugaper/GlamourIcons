using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameObjectPulseEffect : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private float pulseSpeed = 2.0f;
    [SerializeField] private float minAlpha = 0.0f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float maxAlphaHoldTime = 2.0f;

    private bool isVisible = true;
    private CanvasGroup canvasGroup;
    private Coroutine pulseCoroutine;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        pulseCoroutine = StartCoroutine(PulseOpacity());

        if (button != null)
        {
            button.onClick.AddListener(HideGameObject);
        }
    }

    IEnumerator PulseOpacity()
    {
        float t = 0;
        bool increasing = true;

        while (isVisible)
        {
            if (increasing)
            {
                t += Time.deltaTime * pulseSpeed;
                if (t >= 1.0f)
                {
                    t = 1.0f;
                    canvasGroup.alpha = maxAlpha;

                    // Mantener opacidad máxima durante el tiempo configurado
                    yield return new WaitForSeconds(maxAlphaHoldTime);

                    increasing = false;
                }
            }
            else
            {
                t -= Time.deltaTime * pulseSpeed;
                if (t <= 0.0f)
                {
                    t = 0.0f;
                    increasing = true;
                }
            }

            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            yield return null;
        }
    }

    public void HideGameObject()
    {
        isVisible = false;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        gameObject.SetActive(false);
    }
}
