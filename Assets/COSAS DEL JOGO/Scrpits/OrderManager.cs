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

    private int currentOrderIndex = -1;
    private Coroutine slideCoroutine;
    private bool orderActive = false;
    private PizzaOrder currentOrder;

    private int pizzasCompleted = 0;
    private float gameStartTime;
    [SerializeField] private int maxPizzasPerGame = 3;

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
        currentOrder = availableOrders[currentOrderIndex];

        // Update UI elements
        if (pizzaOrderImage != null)
            pizzaOrderImage.sprite = currentOrder.pizzaImage;

        // Update order name if text component exists
        if (orderNameText != null)
            orderNameText.text = currentOrder.orderName;

        // Update ingredients list if text component exists
        if (ingredientsText != null)
        {
            string ingredientsList = "Ingredientes necesarios:\n";
            foreach (string ingredient in currentOrder.requiredIngredients)
            {
                ingredientsList += "- " + ingredient + "\n";
            }
            ingredientsText.text = ingredientsList;
        }

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
        return currentOrder != null ? currentOrder.orderName : "";
    }

    // Método para verificar si un pedido está activo
    public bool IsOrderActive()
    {
        return orderActive;
    }

    public void RegisterPizzaResult(List<string> addedIngredients)
    {
        if (currentOrder == null) return;
        
        List<string> correctIngredients = new List<string>();
        List<string> incorrectIngredients = new List<string>();

        // Contar ingredientes correctos
        foreach (string ingredient in addedIngredients)
        {
            if (currentOrder.requiredIngredients.Contains(ingredient))
            {
                correctIngredients.Add(ingredient);
            }
            else
            {
                incorrectIngredients.Add(ingredient);
            }
        }
        
        bool completed = AreAllIngredientsAdded(addedIngredients);
        int pizzaScore = correctIngredients.Count * 10 - incorrectIngredients.Count * 5;
        if (completed) pizzaScore += 100; // Bonus por completar
        
        // Crear y guardar los datos de la pizza
        PizzaDataManager.PizzaData pizzaData = new PizzaDataManager.PizzaData(
            currentOrder.orderName,
            correctIngredients.Count,
            incorrectIngredients.Count,
            pizzaScore,
            completed
        );
        
        // Guardar en PlayerPrefs
        PizzaDataManager.SavePizzaData(pizzaData, pizzasCompleted);
        
        // Incrementar contador de pizzas
        pizzasCompleted++;
        
        // Si hemos alcanzado el límite, ir a la escena de resultados
        if (pizzasCompleted >= maxPizzasPerGame)
        {
            // Guardar tiempo total de juego
            float gameTime = Time.time - gameStartTime;
            PizzaDataManager.SaveGameTime(gameTime);
            
            // Cargar escena de resultados
            UnityEngine.SceneManagement.SceneManager.LoadScene("ResultsScene");
        }
    }

    // Método para notificar que un pedido ha sido completado correctamente
    public void CompleteOrder()
    {
        if (currentOrder == null || !orderActive) return;
        
        // Obtener la lista de ingredientes actuales (debes pasar esto desde ImageTracker)
        List<string> currentIngredients = GetCurrentIngredients();
        
        // Registrar resultados
        RegisterPizzaResult(currentIngredients);
        
        // Notificar al ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OrderCompleted(currentOrder.orderName, currentOrder.requiredIngredients.Count);
        }
        
        HideCurrentOrder();
        
        // Si no hemos terminado, mostrar el siguiente pedido automáticamente
        if (pizzasCompleted < maxPizzasPerGame)
        {
            Invoke("ShowNextRandomOrder", 1.5f);
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
}