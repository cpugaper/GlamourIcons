using UnityEngine;
using TMPro;

public class PizzaReceiptItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI ingredientsText;

    public void SetupItem(string name, int score, string status, int correctIngredients, int incorrectIngredients)
    {
        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score.ToString();
        if (statusText != null) 
        {
            statusText.text = status;
            statusText.color = status == "Completada" ? Color.green : Color.red;
        }
        
        if (ingredientsText != null)
        {
            ingredientsText.text = $"Ingredientes correctos: {correctIngredients}\n" +
                                  $"Ingredientes incorrectos: {incorrectIngredients}";
        }
    }
}