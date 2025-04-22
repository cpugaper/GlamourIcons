using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PizzaValidationManager : MonoBehaviour
{
    [Header("Configuración de validación")]
    [Range(0f, 1f)]
    [Tooltip("Porcentaje mínimo de ingredientes correctos para aprobar la pizza.")]
    public float approvalThreshold = 0.65f;

    [Header("UI References")]
    [Tooltip("Texto donde se mostrará ‘APROBADO’ o ‘FALLIDO’")]
    public TextMeshProUGUI resultText;

    private List<string> approvedOrders = new List<string>();

    public bool ValidatePizza(string orderName, List<string> required, List<string> actual)
    {
        if (required == null || required.Count == 0)
        {
            Debug.LogWarning("No hay ingredientes requeridos para validar.");
            return false;
        }

        // Conteo de coincidencias
        int correctCount = required.Intersect(actual).Count();
        float ratio = (float)correctCount / required.Count;

        bool approved = ratio >= approvalThreshold;

        // Guardar si está aprobado
        if (approved && !approvedOrders.Contains(orderName))
            approvedOrders.Add(orderName);

        // Actualizar UI
        if (resultText != null)
        {
            resultText.text = approved ? "APROBADO" : "FALLIDO";
            resultText.color = approved ? Color.green : Color.red;
        }

        Debug.Log($"Pizza '{orderName}': {correctCount}/{required.Count} ({ratio*100:0.0}%) -> {(approved? "APROBADA":"FALLIDA")}");
        return approved;
    }

    public List<string> GetApprovedOrders()
    {
        return new List<string>(approvedOrders);
    }
}
