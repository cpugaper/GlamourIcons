using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text orderCountText;
    public TMP_Text feedbackText;

    [Header("Score Settings")]
    [SerializeField] private int baseOrderCompletionPoints = 100;
    [SerializeField] private int pointsPerIngredient = 20;
    [SerializeField] private float feedbackDisplayTime = 2.0f;

    private int score = 0;
    private int completedOrders = 0;
    private float feedbackTimer = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
        
        // Inicializar texto de feedback vacío
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void Update()
    {
        // Control del tiempo de visualización del feedback
        if (feedbackText != null && !string.IsNullOrEmpty(feedbackText.text))
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
            {
                feedbackText.text = "";
            }
        }
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreUI();
        
        // Mostrar feedback visual
        ShowFeedback(points > 0 ? "+" + points.ToString() : points.ToString(), points > 0);
    }

    public void OrderCompleted(string orderName, int correctIngredients, int totalIngredients, float accuracy)
    {
        completedOrders++;

        int basePoints = 100;
        int ingredientBonus = correctIngredients * 20;
        int accuracyBonus = Mathf.RoundToInt(accuracy * 50); 

        int totalPoints = basePoints + ingredientBonus + accuracyBonus;
        score += totalPoints;

        GameData.TotalScore = score;
        GameData.CompletedOrders = completedOrders;

        string message = $"¡{orderName} completada!\n";
        message += $"+{basePoints} (base)\n";
        message += $"+{ingredientBonus} ({correctIngredients}/{totalIngredients} ingredientes)\n";
        message += $"+{accuracyBonus} ({(accuracy * 100):0}% precisión)";

        ShowFeedback(message, true);
        UpdateScoreUI();
    }

    public void AddPoints(int points, string reason)
    {
        score += points;
        UpdateScoreUI();
        ShowFeedback($"{points} puntos: {reason}", points > 0);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        
        if (orderCountText != null)
        {
            orderCountText.text = "Pedidos: " + completedOrders.ToString();
        }
    }

    private void ShowFeedback(string message, bool isPositive)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isPositive ? Color.green : Color.red;
            feedbackTimer = feedbackDisplayTime;
        }
    }

    public void ResetScore()
    {
        score = 0;
        completedOrders = 0;
        UpdateScoreUI();
        
        ShowFeedback("Score reiniciado", false);
    }

    public int GetScore()
    {
        return score;
    }

    public int GetCompletedOrders()
    {
        return completedOrders;
    }
}