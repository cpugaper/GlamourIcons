using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class OrderManager : MonoBehaviour
{

    public static OrderManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    [System.Serializable]
    public class PizzaOrder
    {
        public string orderName;
        public Sprite pizzaImage;
        public List<string> requiredIngredients = new List<string>();  // Lista de nombres de ingredientes necesarios
    }

    [Header("Order Settings")]
    [SerializeField] private List<PizzaOrder> availableOrders = new List<PizzaOrder>();
    [SerializeField] private RectTransform orderPanel;
    [SerializeField] private Image pizzaOrderImage;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.3f;
    [SerializeField] private TextMeshProUGUI orderNameText;
    [SerializeField] private TextMeshProUGUI ingredientsText;


    [Header("Position Settings")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(300f, 0f);
    [SerializeField] private Vector2 visiblePosition = Vector2.zero;

    [Header("Validation")]
    [SerializeField] private PizzaValidationManager validationManager;

    private int currentOrderIndex = -1;
    private Coroutine slideCoroutine;
    private bool orderActive = false;
    private PizzaOrder currentOrder;

    private int pizzasCompleted = 0;
    private float gameStartTime;
    [SerializeField] private int maxPizzasPerGame = 3;

    private List<string> currentAddedIngredients = new List<string>();
    private int currentPizzaScore = 0;

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

        GameData.PizzaApprovals.Clear();

        if (availableOrders.Count == 0)
        {
            Debug.LogError("No pizza orders configured in Order Manager!");
            return;
        }
        orderPanel.anchoredPosition = hiddenPosition;

        GameData.TotalOrders = maxPizzasPerGame;

        GameData.PizzaApprovals.Clear();
        GameData.TotalScore = 0;
        GameData.CompletedOrders = 0;
        GameData.IngredientsPerPizza.Clear();
        GameData.CurrentPizzaIngredients.Clear(); 
    }

    public void ShowOrderForNewPizza()
    {
        ShowNextRandomOrder();
    }

    public void ShowNextRandomOrder()
    {
        if (availableOrders.Count == 0) return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, availableOrders.Count);
        } while (newIndex == currentOrderIndex && availableOrders.Count > 1);

        currentOrderIndex = newIndex;
        currentOrder = availableOrders[currentOrderIndex];

        if (pizzaOrderImage != null)
            pizzaOrderImage.sprite = currentOrder.pizzaImage;

        if (orderNameText != null)
            orderNameText.text = currentOrder.orderName;

        if (ingredientsText != null)
        {
            string ingredientsList = "Ingredientes necesarios:\n";
            foreach (string ingredient in currentOrder.requiredIngredients)
            {
                ingredientsList += "- " + ingredient + "\n";
            }
            ingredientsText.text = ingredientsList;
        }

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlidePanel(hiddenPosition, visiblePosition, slideInDuration));
        orderActive = true;
    }

    public void HideCurrentOrder()
    {
        if (!orderActive) return;

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

    // Método para verificar si un ingrediente es parte del pedido actual
    public bool IsIngredientRequired(string ingredientName)
    {
        if (currentOrder == null || !orderActive) return false;
        return currentOrder.requiredIngredients.Contains(ingredientName);
    }

    // Método para verificar si todos los ingredientes han sido añadidos
    public bool AreAllIngredientsAdded(List<string> addedIngredients)
    {
        if (currentOrder == null || !orderActive) return false;
        
        // Verifica que todos los ingredientes requeridos están en la lista de añadidos
        foreach (string requiredIngredient in currentOrder.requiredIngredients)
        {
            if (!addedIngredients.Contains(requiredIngredient))
            {
                return false;
            }
        }
        
        return true;
    }

    // Método para obtener los ingredientes necesarios para el pedido actual
    public List<string> GetRequiredIngredients()
    {
        if (currentOrder == null) return new List<string>();
        return new List<string>(currentOrder.requiredIngredients);
    }

    // Método para obtener el nombre del pedido actual
    public string GetCurrentOrderName()
    {
        string orderName = currentOrder != null ? currentOrder.orderName : "NINGÚN PEDIDO ACTIVO";
        Debug.Log($"Obteniendo nombre del pedido actual: {orderName}");
        return orderName;
    }

    // Método para verificar si un pedido está activo
    public bool IsOrderActive()
    {
        return orderActive;
    }

    // Método para notificar que un pedido ha sido completado correctamente
    public void CompleteOrder()
    {
        if (currentOrder == null || !orderActive) return;

        int correctIngredients = 0;
        foreach (string ingredient in currentAddedIngredients)
        {
            if (currentOrder.requiredIngredients.Contains(ingredient))
            {
                correctIngredients++;
            }
        }

        float accuracy = (float)correctIngredients / currentOrder.requiredIngredients.Count;
        bool approved = accuracy >= 0.65f; 

        // Guardar resultados
        GameData.PizzaApprovals.Add(approved);
        GameData.TotalScore += currentPizzaScore;
        GameData.CompletedOrders++;

        GameData.IngredientsPerPizza.Add(new List<string>(GameData.CurrentPizzaIngredients));
        Debug.Log($"[GameData] Guardadas {GameData.CurrentPizzaIngredients.Count} ing. para pizza #{pizzasCompleted + 1}");

        GameData.CurrentPizzaIngredients.Clear();
        currentAddedIngredients.Clear();

        // Asignar puntuación
        if (approved && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OrderCompleted(
                currentOrder.orderName,
                correctIngredients,
                currentOrder.requiredIngredients.Count,
                accuracy
            );
        }

        // Resetear para siguiente pizza
        currentAddedIngredients.Clear();
        currentPizzaScore = 0;
        pizzasCompleted++;

        HideCurrentOrder();

        if (pizzasCompleted < maxPizzasPerGame)
        {
            Invoke("ShowNextRandomOrder", 1.5f);
        }
        else
        {
            SceneManager.LoadScene("Results");
        }

        GameData.IngredientsPerPizza.Add(new List<string>(GameData.CurrentPizzaIngredients));
        GameData.CurrentPizzaIngredients.Clear();

    }
    public void AddIngredient(string ingredientName)
    {
        currentAddedIngredients.Add(ingredientName);
        GameData.CurrentPizzaIngredients.Add(ingredientName);   // ← guardar para resultados
        Debug.Log($"[GameData] Ingrediente añadido: {ingredientName} (total actual: {GameData.CurrentPizzaIngredients.Count})");
        if (IsIngredientRequired(ingredientName))
        {
            currentPizzaScore += 20; 
            ScoreManager.Instance?.AddPoints(20, $"Ingrediente correcto: {ingredientName}");
        }
        else
        {
                currentPizzaScore -= 10; 
                ScoreManager.Instance?.AddPoints(-10, $"Ingrediente incorrecto: {ingredientName}");
        }
    }

    // Método para obtener ingredientes actuales (este método debe ser llamado desde ImageTracker)
    public void SetCurrentIngredients(List<string> ingredients)
    {
        _currentIngredients = ingredients;
    }

    private List<string> _currentIngredients = new List<string>();

    private List<string> GetCurrentIngredients()
    {
        return _currentIngredients;
    }

    
    public PizzaOrder GetCurrentOrder()
    {
        return currentOrder;
    }
}