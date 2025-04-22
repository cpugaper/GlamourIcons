using System.Collections.Generic;
using UnityEngine;
using System;

public class PizzaDataManager : MonoBehaviour
{
    // Clase que representa una pizza completada
    [Serializable]
    public class PizzaData
    {
        public string pizzaName;
        public int correctIngredients;
        public int incorrectIngredients;
        public int score;
        public bool completed;

        // Constructor para facilitar la creación
        public PizzaData(string name, int correct, int incorrect, int points, bool isComplete)
        {
            pizzaName = name;
            correctIngredients = correct;
            incorrectIngredients = incorrect;
            score = points;
            completed = isComplete;
        }
    }

    // Guarda los datos de una pizza usando PlayerPrefs
    public static void SavePizzaData(PizzaData pizza, int index)
    {
        PlayerPrefs.SetString($"Pizza_{index}_Name", pizza.pizzaName);
        PlayerPrefs.SetInt($"Pizza_{index}_Correct", pizza.correctIngredients);
        PlayerPrefs.SetInt($"Pizza_{index}_Incorrect", pizza.incorrectIngredients);
        PlayerPrefs.SetInt($"Pizza_{index}_Score", pizza.score);
        PlayerPrefs.SetInt($"Pizza_{index}_Completed", pizza.completed ? 1 : 0);
        
        // Actualizar contador de pizzas
        int currentCount = PlayerPrefs.GetInt("TotalPizzasCount", 0);
        if (index >= currentCount)
        {
            PlayerPrefs.SetInt("TotalPizzasCount", index + 1);
        }
        
        // Actualizar puntuación total
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        PlayerPrefs.SetInt("TotalScore", totalScore + pizza.score);
        
        // Actualizar pizzas completadas
        if (pizza.completed)
        {
            int completedCount = PlayerPrefs.GetInt("CompletedPizzasCount", 0);
            PlayerPrefs.SetInt("CompletedPizzasCount", completedCount + 1);
        }
        
        PlayerPrefs.Save();
    }

    // Carga los datos de una pizza específica
    public static PizzaData LoadPizzaData(int index)
    {
        if (!PlayerPrefs.HasKey($"Pizza_{index}_Name"))
            return null;
            
        string name = PlayerPrefs.GetString($"Pizza_{index}_Name");
        int correct = PlayerPrefs.GetInt($"Pizza_{index}_Correct");
        int incorrect = PlayerPrefs.GetInt($"Pizza_{index}_Incorrect");
        int score = PlayerPrefs.GetInt($"Pizza_{index}_Score");
        bool completed = PlayerPrefs.GetInt($"Pizza_{index}_Completed") == 1;
        
        return new PizzaData(name, correct, incorrect, score, completed);
    }

    // Carga todos los datos de pizzas
    public static List<PizzaData> LoadAllPizzaData()
    {
        List<PizzaData> pizzas = new List<PizzaData>();
        int count = PlayerPrefs.GetInt("TotalPizzasCount", 0);
        
        for (int i = 0; i < count; i++)
        {
            PizzaData pizza = LoadPizzaData(i);
            if (pizza != null)
            {
                pizzas.Add(pizza);
            }
        }
        
        return pizzas;
    }

    // Obtiene el total de puntos
    public static int GetTotalScore()
    {
        return PlayerPrefs.GetInt("TotalScore", 0);
    }

    // Obtiene el conteo de pizzas completadas
    public static int GetCompletedPizzasCount()
    {
        return PlayerPrefs.GetInt("CompletedPizzasCount", 0);
    }

    // Obtiene el total de pizzas
    public static int GetTotalPizzasCount()
    {
        return PlayerPrefs.GetInt("TotalPizzasCount", 0);
    }

    // Limpia todos los datos guardados
    public static void ClearAllData()
    {
        int count = PlayerPrefs.GetInt("TotalPizzasCount", 0);
        
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"Pizza_{i}_Name");
            PlayerPrefs.DeleteKey($"Pizza_{i}_Correct");
            PlayerPrefs.DeleteKey($"Pizza_{i}_Incorrect");
            PlayerPrefs.DeleteKey($"Pizza_{i}_Score");
            PlayerPrefs.DeleteKey($"Pizza_{i}_Completed");
        }
        
        PlayerPrefs.DeleteKey("TotalPizzasCount");
        PlayerPrefs.DeleteKey("TotalScore");
        PlayerPrefs.DeleteKey("CompletedPizzasCount");
        PlayerPrefs.Save();
    }
    
    // Guarda el tiempo de juego
    public static void SaveGameTime(float seconds)
    {
        PlayerPrefs.SetFloat("GameTime", seconds);
        PlayerPrefs.Save();
    }
    
    // Carga el tiempo de juego
    public static float GetGameTime()
    {
        return PlayerPrefs.GetFloat("GameTime", 0f);
    }
}