using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsDisplay : MonoBehaviour
{
    [Tooltip("Arrastra aquí tu TextMeshProUGUI para mostrar los resultados.")]
    public TextMeshProUGUI resultsText;

    void Start()
    {
        // Solo en la escena de resultados
        if (SceneManager.GetActiveScene().name != "Results") return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("RESULTADOS FINALES");
        sb.AppendLine($"Puntuación total: {GameData.TotalScore}");
        sb.AppendLine($"Pedidos completados: {GameData.CompletedOrders}/{GameData.TotalOrders}");
        sb.AppendLine("---------------------");

        // Recorremos el total de pedidos para mostrar estado e ingredientes
        for (int i = 0; i < GameData.TotalOrders; i++)
        {
            bool approved = i < GameData.PizzaApprovals.Count && GameData.PizzaApprovals[i];
            string status = approved
                ? "APROBADA"
                : (i < GameData.PizzaApprovals.Count ? "FALLIDA" : "NO JUGADA");
            string color = approved
                ? "green"
                : (i < GameData.PizzaApprovals.Count ? "red" : "yellow");

            sb.AppendLine($"Pizza {i + 1}: <color={color}>{status}</color>");

            // Ingredientes que puso el jugador
            string ingredientes = (i < GameData.IngredientsPerPizza.Count)
                ? string.Join(", ", GameData.IngredientsPerPizza[i])
                : "—";
            sb.AppendLine($"    Ingredientes puestos: {ingredientes}");
        }

        if (resultsText != null)
        {
            resultsText.richText = true;
            resultsText.text = sb.ToString();
        }
    }
}