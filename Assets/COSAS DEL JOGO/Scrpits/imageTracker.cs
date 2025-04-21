using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    [System.Serializable]
    public class IngredientMapping
    {
        public string imageName;
        public GameObject visualReferencePrefab;
        public float spawnScale = 1.0f;
    }

    [Header("AR Components")]
    private ARTrackedImageManager trackedImageManager;

    [Header("Ingredient Settings")]
    [SerializeField] private List<IngredientMapping> ingredientMappings = new List<IngredientMapping>();
    [SerializeField] private float ingredientSpawnHeight = 0.02f;
    [SerializeField] private float detectionRadius = 0.15f;

    private readonly Dictionary<string, GameObject> referenceIngredients = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> pizzaIngredients = new Dictionary<string, GameObject>();

    private PizzaSpawner pizzaSpawner;

    private void Awake()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin (Mobile AR)");

        if (xrOrigin != null)
        {
            trackedImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();

            if (trackedImageManager == null)
            {
                trackedImageManager = xrOrigin.AddComponent<ARTrackedImageManager>();
                Debug.LogWarning("Se ha añadido un ARTrackedImageManager a XR Origin");
            }
        }
        else
        {
            Debug.LogError("No se pudo encontrar el objeto XR Origin (Mobile AR) en la escena");
        }

        pizzaSpawner = FindAnyObjectByType<PizzaSpawner>();
        if (pizzaSpawner != null)
        {
            pizzaSpawner.OnPizzaDestroyed += ClearPizzaIngredients;
        }
        else
        {
            Debug.LogError("No se encontro PizzaSpawner en la escena");
        }
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
        else
        {
            Debug.LogError("ARTrackedImageManager no esta disponible");
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        if (pizzaSpawner != null)
        {
            pizzaSpawner.OnPizzaDestroyed -= ClearPizzaIngredients;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateIngredientPosition(trackedImage);
                CheckPizzaProximity(trackedImage);
            }
            else
            {
                DeactivateReferenceIngredient(trackedImage.referenceImage.name);
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            DeactivateReferenceIngredient(trackedImage.referenceImage.name);
        }

    }
    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        IngredientMapping mapping = GetMappingByName(imageName);

        if (mapping != null)
        {
            if (referenceIngredients.TryGetValue(imageName, out GameObject ingredient))
            {
                ingredient.SetActive(true);
                ingredient.transform.position = trackedImage.transform.position;
                ingredient.transform.rotation = trackedImage.transform.rotation;
            }
            else
            {
                GameObject newIngredient = Instantiate(mapping.visualReferencePrefab,
                                                      trackedImage.transform.position,
                                                      trackedImage.transform.rotation);

                newIngredient.transform.localScale = Vector3.one * mapping.spawnScale;

                referenceIngredients.Add(imageName, newIngredient);

                Debug.Log($"Ingrediente detectado: {imageName}");
            }
        }
        else
        {
            Debug.LogWarning($"Imagen detectada '{imageName}' sin configuracion de ingrediente");
        }
    }

    private void UpdateIngredientPosition(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (referenceIngredients.TryGetValue(imageName, out GameObject ingredient) && ingredient.activeSelf)
        {
            Vector3 targetPosition = Vector3.Lerp(ingredient.transform.position,
                                                trackedImage.transform.position,
                                                Time.deltaTime * 8);

            Quaternion targetRotation = Quaternion.Slerp(ingredient.transform.rotation,
                                                       trackedImage.transform.rotation,
                                                       Time.deltaTime * 8);

            ingredient.transform.position = targetPosition;
            ingredient.transform.rotation = targetRotation;
            ingredient.SetActive(true);
        }
    }

    private void CheckPizzaProximity(ARTrackedImage trackedImage)
    {
        if (pizzaSpawner == null || pizzaSpawner.currentPizzaBase == null) return;
        string imageName = trackedImage.referenceImage.name;

        if (pizzaIngredients.ContainsKey(imageName))
        {
            return;
        }

        float distance = Vector3.Distance(trackedImage.transform.position, pizzaSpawner.currentPizzaBase.transform.position);

        if (distance <= detectionRadius)
        {
            SpawnIngredientOnPizza(imageName);
            Debug.Log($"Ingrediente {imageName} añadido a la pizza");
        }
    }

    private void SpawnIngredientOnPizza(string imageName)
    {
        IngredientMapping mapping = GetMappingByName(imageName);
        if (mapping == null || mapping.visualReferencePrefab == null || pizzaSpawner.currentPizzaBase == null)
            return;

        Vector3 spawnPosition = pizzaSpawner.currentPizzaBase.transform.position +
                              (Vector3.up * ingredientSpawnHeight);

        GameObject newIngredient = Instantiate(
            mapping.visualReferencePrefab,
            spawnPosition,
            pizzaSpawner.currentPizzaBase.transform.rotation,
            pizzaSpawner.currentPizzaBase.transform
        );

        newIngredient.transform.localScale = Vector3.one * mapping.spawnScale;
        pizzaIngredients.Add(imageName, newIngredient);
    }

    private void DeactivateReferenceIngredient(string imageName)
    {
        if (referenceIngredients.TryGetValue(imageName, out GameObject ingredient))
        {
            ingredient.SetActive(false);
        }
    }

    private void ClearPizzaIngredients()
    {
        foreach (var ingredient in pizzaIngredients.Values)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        pizzaIngredients.Clear();
        Debug.Log("Todos los ingredientes de la pizza han sido eliminados");
    }

    public IngredientMapping GetMappingByName(string imageName)
    {
        foreach (var mapping in ingredientMappings)
        {
            if (mapping.imageName == imageName)
            {
                return mapping;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        foreach (var ingredient in referenceIngredients.Values)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        foreach (var ingredient in pizzaIngredients.Values)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        referenceIngredients.Clear();
        pizzaIngredients.Clear();
    }
}