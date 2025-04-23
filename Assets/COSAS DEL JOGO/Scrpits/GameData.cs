using UnityEngine;
using System.Collections.Generic;

public static class GameData
{
    public static List<bool> PizzaApprovals = new List<bool>();
    public static int TotalScore = 0;
    public static int CompletedOrders = 0;
    public static int TotalOrders = 3; 

    public static void Reset()
    {
        PizzaApprovals.Clear();
        TotalScore = 0;
        CompletedOrders = 0;
    }
}