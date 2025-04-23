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

    public bool ValidatePizza(List<string> actualIngredients)
    {
        // Obtener pedido activo
        string currentOrderName = OrderManager.Instance.GetCurrentOrderName();
        List<string> requiredIngredients = OrderManager.Instance.GetRequiredIngredients();

        if (requiredIngredients == null || requiredIngredients.Count == 0)
        {
            Debug.LogWarning("No hay ingredientes requeridos para validar.");
            return false;
        }

        // Conteo de ingredientes correctos
        int correctCount = requiredIngredients.Intersect(actualIngredients).Count();
        float ratio = (float)correctCount / requiredIngredients.Count;
        bool approved = ratio >= approvalThreshold;

        // Guardar si está aprobado y no se había guardado antes
        if (approved && !approvedOrders.Contains(currentOrderName))
            approvedOrders.Add(currentOrderName);

        // Actualizar UI
        if (resultText != null)
        {
            resultText.text = approved ? "APROBADO" : "FALLIDO";
            resultText.color = approved ? Color.green : Color.red;
        }

        Debug.Log($"Pizza '{currentOrderName}': {correctCount}/{requiredIngredients.Count} ({ratio * 100:0.0}%) -> {(approved ? "APROBADA" : "FALLIDA")}");
        return approved;
    }

    public List<string> GetApprovedOrders()
    {
        return new List<string>(approvedOrders);
    }
}