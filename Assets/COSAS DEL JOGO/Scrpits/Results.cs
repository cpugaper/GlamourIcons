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
        // Asegurémonos de estar en la escena correcta
        if (SceneManager.GetActiveScene().name != "Results") return;

        // Construimos el texto multilínea
        var sb = new StringBuilder();
        sb.AppendLine("RESULTADOS FINAL:");
        int count = GameData.PizzaApprovals.Count;
        for (int i = 0; i < count; i++)
        {
            bool ok = GameData.PizzaApprovals[i];
            string color = ok ? "green" : "red";
            string word  = ok ? "APROBADO" : "FALLIDO";
            // usando Rich Text para colorear
            sb.AppendLine($"{i + 1}. <color={color}>{word}</color>");
        }
        // Si por algún motivo hay menos de 3 entradas, podemos rellenar:
        for (int i = count; i < 3; i++)
        {
            sb.AppendLine($"{i + 1}. <color=yellow>NO JUGADA</color>");
        }

        // Asignamos al TMP
        if (resultsText != null)
        {
            resultsText.richText = true;
            resultsText.text = sb.ToString();
        }
    }
}
