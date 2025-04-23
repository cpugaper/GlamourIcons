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
        if (SceneManager.GetActiveScene().name != "Results") return;

        var sb = new StringBuilder();
        sb.AppendLine("RESULTADOS FINALES");
        sb.AppendLine($"Puntuación total: {GameData.TotalScore}");
        sb.AppendLine($"Pedidos completados: {GameData.CompletedOrders}/{GameData.TotalOrders}");
        sb.AppendLine("---------------------");

        for (int i = 0; i < GameData.TotalOrders; i++)
        {
            bool ok = i < GameData.PizzaApprovals.Count ? GameData.PizzaApprovals[i] : false;
            string status = ok ? "APROBADA" : i < GameData.PizzaApprovals.Count ? "FALLIDA" : "NO JUGADA";
            string color = ok ? "green" : i < GameData.PizzaApprovals.Count ? "red" : "yellow";

            sb.AppendLine($"Pizza {i + 1}: <color={color}>{status}</color>");
        }

        if (resultsText != null)
        {
            resultsText.richText = true;
            resultsText.text = sb.ToString();
        }

        for (int i = 0; i < GameData.TotalOrders; i++)
        {
            bool ok = i < GameData.PizzaApprovals.Count && GameData.PizzaApprovals[i];
            // … línea de status …
            
            // — NUEVO — ingredientes añadidos:
            string ingredientes = (i < GameData.IngredientsPerPizza.Count)
                ? string.Join(", ", GameData.IngredientsPerPizza[i])
                : "—";
            sb.AppendLine($"Ingredientes puestos: {ingredientes}");
        }

        if (resultsText != null)
        {
            resultsText.richText = true;
            resultsText.text = sb.ToString();
        }
    }
}
