using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI; // Para elementos UI opcionales

public class PizzaSpawner : MonoBehaviour
{
    [Header("AR Components")]
    // Estos componentes los buscaremos automáticamente en el XR Origin
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    [Header("Pizza Components")]
    [SerializeField] private GameObject pizzaBasePrefab;
    [SerializeField] private List<GameObject> ingredientPrefabs;

    [Header("Settings")]
    [SerializeField] private float minPlaneSize = 0.4f; // Tamaño mínimo para una superficie válida
    [SerializeField] private LayerMask pizzaBaseLayer;
    [SerializeField] private int maxPizzaCount = 1; // Máximo número de pizzas permitidas
    [SerializeField] private Text statusText; // Opcional: para mostrar mensajes al usuario

    private int currentPizzaCount = 0;
    private GameObject currentPizzaBase;
    private bool pizzaBaseCreated = false;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private Camera arCamera;

    private void Awake()
    {
        // Buscar los componentes AR directamente desde XR Origin
        GameObject xrOrigin = GameObject.Find("XR Origin (Mobile AR)");

        if (xrOrigin != null)
        {
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
            planeManager = xrOrigin.GetComponent<ARPlaneManager>();

            if (raycastManager == null)
                Debug.LogError("No se pudo encontrar ARRaycastManager en XR Origin");

            if (planeManager == null)
                Debug.LogError("No se pudo encontrar ARPlaneManager en XR Origin");
        }
        else
        {
            Debug.LogError("No se pudo encontrar el objeto XR Origin (Mobile AR) en la escena");
        }
    }

    private void Start()
    {
        // Obtener la cámara principal (AR Camera)
        arCamera = Camera.main;
        currentPizzaCount = 0;

        UpdateStatusText();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Si no hay una pizza base creada Y no hemos llegado al límite, intentar crear una
                if (!pizzaBaseCreated && currentPizzaCount < maxPizzaCount)
                {
                    TryCreatePizzaBase(touch.position);
                }
                // Si ya hay una pizza base, intentar añadir ingrediente
                else if (pizzaBaseCreated)
                {
                    TryAddIngredient(touch.position);
                }
                // Si ya se ha alcanzado el límite, mostrar mensaje
                else if (currentPizzaCount >= maxPizzaCount)
                {
                    Debug.Log("¡Ya tienes una pizza! Usa el botón 'Completar Pizza' o 'Reiniciar' para continuar.");
                    if (statusText != null)
                        statusText.text = "¡Ya tienes una pizza! Complétala o reinicia para crear otra.";
                }
            }
        }
    }

    private void TryCreatePizzaBase(Vector2 touchPosition)
    {
        // Verificar que el raycastManager existe
        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager no está asignado");
            return;
        }

        // Verificar nuevamente que no hayamos alcanzado el límite
        if (currentPizzaCount >= maxPizzaCount)
        {
            Debug.Log("No puedes crear más pizzas hasta que completes o elimines la actual.");
            return;
        }

        // Lanzar un rayo desde la posición del toque para detectar planos AR
        if (raycastManager.Raycast(touchPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            // Verificar el primer hit
            ARRaycastHit hit = raycastHits[0];
            ARPlane plane = planeManager.GetPlane(hit.trackableId);

            // Comprobar si el plano es lo suficientemente grande
            if (IsPlaneValidForPizzaBase(plane))
            {
                // Crear la base de pizza en la posición del hit
                Pose hitPose = hit.pose;
                currentPizzaBase = Instantiate(pizzaBasePrefab, hitPose.position, hitPose.rotation);

                // Ajustar la orientación para que quede horizontal
                currentPizzaBase.transform.up = hit.pose.up;

                pizzaBaseCreated = true;
                currentPizzaCount++;
                UpdateStatusText();

                // Opcional: Desactivar la visualización de planos una vez colocada la pizza
                SetPlanesVisibility(false);
            }
            else
            {
                Debug.Log("La superficie no es lo suficientemente grande para la pizza");
                if (statusText != null)
                    statusText.text = "Busca una superficie más grande para la pizza";
            }
        }
    }

    private bool IsPlaneValidForPizzaBase(ARPlane plane)
    {
        // Si plane es null, no podemos validarlo
        if (plane == null)
            return false;

        // Comprobar si el plano es horizontal hacia arriba
        if (plane.alignment != PlaneAlignment.HorizontalUp)
            return false;

        // Comprobar si el plano es lo suficientemente grande
        return plane.size.x >= minPlaneSize && plane.size.y >= minPlaneSize;
    }

    private void TryAddIngredient(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        // Verificar si el rayo golpea la base de pizza
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, pizzaBaseLayer))
        {
            // Seleccionar un ingrediente aleatorio de la lista (si hay ingredientes disponibles)
            if (ingredientPrefabs != null && ingredientPrefabs.Count > 0)
            {
                GameObject ingredientPrefab = ingredientPrefabs[Random.Range(0, ingredientPrefabs.Count)];

                // Instanciar el ingrediente en la posición del hit
                GameObject ingredient = Instantiate(ingredientPrefab, hit.point, Quaternion.identity);

                // Hacer que el ingrediente sea hijo de la base de pizza
                ingredient.transform.SetParent(currentPizzaBase.transform);

                // Orientar el ingrediente según la normal de la superficie
                ingredient.transform.up = hit.normal;

                // Añadir una rotación aleatoria al ingrediente
                ingredient.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
            }
            else
            {
                Debug.LogWarning("No hay ingredientes configurados en el PizzaSpawner");
            }
        }
    }

    // Método para resetear la escena y permitir crear una nueva pizza
    public void ResetPizza()
    {
        if (currentPizzaBase != null)
        {
            Destroy(currentPizzaBase);
            currentPizzaBase = null;
        }

        pizzaBaseCreated = false;
        currentPizzaCount = 0;
        UpdateStatusText();

        // Reactivar los planos
        SetPlanesVisibility(true);
    }

    // Método para mostrar/ocultar planos
    private void SetPlanesVisibility(bool visible)
    {
        if (planeManager != null)
        {
            planeManager.enabled = visible;

            // Recorrer todos los planos existentes
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }
    }

    // Método para completar la pizza actual (para simular que se envía a cocinar)
    public void CompletePizza()
    {
        if (currentPizzaBase != null)
        {
            // Aquí podrías añadir animaciones o efectos
            // Por ejemplo, hacer que la pizza se "deslice" hacia fuera de la pantalla

            // Luego destruir la pizza
            Destroy(currentPizzaBase);
            currentPizzaBase = null;
            pizzaBaseCreated = false;
            currentPizzaCount = 0;
            UpdateStatusText();

            // Reactivar planos para permitir crear una nueva pizza
            SetPlanesVisibility(true);
        }
    }

    // Método para actualizar el texto de estado (opcional)
    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            if (currentPizzaCount <= 0)
                statusText.text = "Toca una superficie para crear la base de pizza";
            else if (pizzaBaseCreated)
                statusText.text = "Toca la masa para añadir ingredientes";
        }
    }

    // Método para seleccionar un ingrediente específico (para UI)
    private int selectedIngredientIndex = 0;

    public void SelectIngredient(int index)
    {
        if (ingredientPrefabs != null && index >= 0 && index < ingredientPrefabs.Count)
        {
            selectedIngredientIndex = index;
        }
    }

    // Versión alternativa de TryAddIngredient que usa el ingrediente seleccionado
    public void TryAddSelectedIngredient(Vector2 touchPosition)
    {
        if (ingredientPrefabs == null || ingredientPrefabs.Count == 0)
            return;

        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, pizzaBaseLayer))
        {
            GameObject ingredientPrefab = ingredientPrefabs[selectedIngredientIndex];
            GameObject ingredient = Instantiate(ingredientPrefab, hit.point, Quaternion.identity);
            ingredient.transform.SetParent(currentPizzaBase.transform);
            ingredient.transform.up = hit.normal;
            ingredient.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
        }
    }
}