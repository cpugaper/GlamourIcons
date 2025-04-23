using UnityEngine;
using System.Collections.Generic;

public static class GameData
{
    public static int TotalOrders;
    public static int CompletedOrders;
    public static int TotalScore;
    public static List<bool> PizzaApprovals = new List<bool>();

    public static List<List<string>> IngredientsPerPizza = new List<List<string>>();

    public static List<string> CurrentPizzaIngredients = new List<string>();
    public static void Reset()
    {
        PizzaApprovals.Clear();
        TotalScore = 0;
        CompletedOrders = 0;
    }   
}

