using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResultsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI receiptTitle;
    [SerializeField] private TextMeshProUGUI dateTimeText;
    [SerializeField] private Transform pizzaItemsContainer;
    [SerializeField] private GameObject pizzaItemPrefab;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI timePlayedText;
    [SerializeField] private TextMeshProUGUI completionRateText;
    
    [Header("Receipt Settings")]
    [SerializeField] private string restaurantName = "Pizza AR";
    [SerializeField] private string thankYouMessage = "¡Gracias por jugar!";

    private void Start()
    {
        PopulateReceipt();
    }

    private void PopulateReceipt()
    {
        // Título y fecha
        receiptTitle.text = restaurantName;
        dateTimeText.text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        
        // Limpiar cualquier item existente
        foreach (Transform child in pizzaItemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Cargar datos de pizzas
        List<PizzaDataManager.PizzaData> pizzaResults = PizzaDataManager.LoadAllPizzaData();
        
        // Mostrar cada pizza en el recibo
        foreach (var pizzaResult in pizzaResults)
        {
            GameObject itemGO = Instantiate(pizzaItemPrefab, pizzaItemsContainer);
            PizzaReceiptItem item = itemGO.GetComponent<PizzaReceiptItem>();
            
            if (item != null)
            {
                item.SetupItem(
                    pizzaResult.pizzaName,
                    pizzaResult.score,
                    pizzaResult.completed ? "Completada" : "Incompleta",
                    pizzaResult.correctIngredients,
                    pizzaResult.incorrectIngredients
                );
            }
        }
        
        // Mostrar totales
        totalScoreText.text = $"Puntuación Total: {PizzaDataManager.GetTotalScore()}";
        
        // Tiempo jugado
        float gameTime = PizzaDataManager.GetGameTime();
        TimeSpan timeSpan = TimeSpan.FromSeconds(gameTime);
        timePlayedText.text = $"Tiempo de juego: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        
        // Tasa completados
        int totalPizzas = PizzaDataManager.GetTotalPizzasCount();
        int completedPizzas = PizzaDataManager.GetCompletedPizzasCount();
        float completionRate = totalPizzas > 0 ? (float)completedPizzas / totalPizzas * 100 : 0;
        completionRateText.text = $"Tasa de éxito: {completionRate:F1}%";
    }
    
    public void RestartGame()
    {
        // Limpiar datos antes de reiniciar
        PizzaDataManager.ClearAllData();
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    public void GoToMainMenu()
    {
        // Limpiar datos antes de volver al menú
        PizzaDataManager.ClearAllData();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}