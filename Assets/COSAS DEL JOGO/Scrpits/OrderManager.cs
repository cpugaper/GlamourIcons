using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [System.Serializable]
    public class PizzaOrder
    {
        public string orderName;
        public Sprite pizzaImage;
    }

    [Header("Order Settings")]
    [SerializeField] private List<PizzaOrder> availableOrders = new List<PizzaOrder>();
    [SerializeField] private RectTransform orderPanel;
    [SerializeField] private Image pizzaOrderImage;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.3f;

    [Header("Position Settings")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(300f, 0f);
    [SerializeField] private Vector2 visiblePosition = Vector2.zero;

    private int currentOrderIndex = -1;
    private Coroutine slideCoroutine;
    private bool orderActive = false;

    private void Start()
    {
        if (availableOrders.Count == 0)
        {
            Debug.LogError("No pizza orders configured in Order Manager!");
            return;
        }

        if (orderPanel != null)
        {
            orderPanel.anchoredPosition = hiddenPosition;
        }
    }

    public void ShowOrderForNewPizza()
    {
        ShowNextRandomOrder();
    }

    public void ShowNextRandomOrder()
    {
        if (availableOrders.Count == 0) return;

        // Select a random order different from the current one
        int newIndex;
        do
        {
            newIndex = Random.Range(0, availableOrders.Count);
        } while (newIndex == currentOrderIndex && availableOrders.Count > 1);

        currentOrderIndex = newIndex;
        PizzaOrder order = availableOrders[currentOrderIndex];

        // Update UI elements
        if (pizzaOrderImage != null)
            pizzaOrderImage.sprite = order.pizzaImage;

        // Start slide in animation
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlidePanel(hiddenPosition, visiblePosition, slideInDuration));
        orderActive = true;
    }

    public void HideCurrentOrder()
    {
        if (!orderActive) return;

        // Start slide out animation
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlidePanel(visiblePosition, hiddenPosition, slideOutDuration));
        orderActive = false;
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Smooth easing
            t = Mathf.SmoothStep(0, 1, t);

            orderPanel.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        orderPanel.anchoredPosition = to;
    }

    public void OnTimerEnded()
    {
        Debug.Log("Timer terminado, ocultando pedido");
        HideCurrentOrder();
    }
}