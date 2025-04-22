using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Pizza AR/Game Data", order = 1)]
public class GameData : ScriptableObject
{
    [System.Serializable]
    public class PizzaResult
    {
        public string pizzaName;
        public int correctIngredients;
        public int incorrectIngredients;
        public int score;
        public bool completed;
    }
    
    // Lista de resultados
    public List<PizzaResult> pizzaResults = new List<PizzaResult>();
    
    // Estadísticas
    public int totalScore;
    public float gameTime;
    
    // Limpiar datos
    public void Reset()
    {
        pizzaResults.Clear();
        totalScore = 0;
        gameTime = 0;
    }
    
    // Añadir resultado de pizza
    public void AddPizzaResult(string name, int correct, int incorrect, int score, bool completed)
    {
        PizzaResult result = new PizzaResult
        {
            pizzaName = name,
            correctIngredients = correct,
            incorrectIngredients = incorrect,
            score = score,
            completed = completed
        };
        
        pizzaResults.Add(result);
        totalScore += score;
    }
    
    // Contar pizzas completadas
    public int GetCompletedPizzasCount()
    {
        int count = 0;
        foreach (var pizza in pizzaResults)
        {
            if (pizza.completed) count++;
        }
        return count;
    }
}