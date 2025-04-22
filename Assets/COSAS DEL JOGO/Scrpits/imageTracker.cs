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

    [Header("Periodic Placement Settings")]
    [SerializeField] private float ingredientPlacementCooldown = 5f;

    private readonly Dictionary<string, GameObject> referenceIngredients = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, List<GameObject>> pizzaIngredients = new Dictionary<string, List<GameObject>>();
    private readonly Dictionary<string, float> lastPlacementTimes = new Dictionary<string, float>();

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

            if (trackedImageManager.referenceLibrary == null ||
                trackedImageManager.referenceLibrary.count == 0)
            {
                Debug.LogError("La librería de imágenes de referencia no está configurada o está vacía");
            }
            else
            {
                Debug.Log($"Librería de imágenes cargada con {trackedImageManager.referenceLibrary.count} imágenes");
            }

            trackedImageManager.requestedMaxNumberOfMovingImages = 5;
            trackedImageManager.trackedImagePrefab = null;
        }
        else
        {
            Debug.LogError("No se pudo encontrar el objeto XR Origin (Mobile AR) en la escena");
        }

        pizzaSpawner = FindAnyObjectByType<PizzaSpawner>();
        if (pizzaSpawner != null)
        {
            pizzaSpawner.OnPizzaDestroyed += ClearPizzaIngredients;
            pizzaSpawner.OnNewPizzaSpawned += HandleNewPizza;
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
            pizzaSpawner.OnNewPizzaSpawned -= HandleNewPizza;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
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

        float distance = Vector3.Distance(trackedImage.transform.position, pizzaSpawner.currentPizzaBase.transform.position);

        if (distance <= detectionRadius)
        {
            float currentTime = Time.time;

            if (!pizzaIngredients.ContainsKey(imageName))
            {
                pizzaIngredients[imageName] = new List<GameObject>();
                lastPlacementTimes[imageName] = 0f;
            }

            if (currentTime - lastPlacementTimes[imageName] >= ingredientPlacementCooldown)
            {
                SpawnIngredientOnPizza(imageName);
                lastPlacementTimes[imageName] = currentTime;
                Debug.Log($"Ingrediente {imageName} añadido a la pizza. Total: {pizzaIngredients[imageName].Count}");
            }
        }
    }

    private void SpawnIngredientOnPizza(string imageName)
    {
        IngredientMapping mapping = GetMappingByName(imageName);
        if (mapping == null || mapping.visualReferencePrefab == null || pizzaSpawner.currentPizzaBase == null)
            return;

        float pizzaRadius = 0.15f;
        Vector2 randomOffset = Random.insideUnitCircle * pizzaRadius;
        Vector3 spawnPosition = pizzaSpawner.currentPizzaBase.transform.position +
                              (Vector3.up * ingredientSpawnHeight) +
                              new Vector3(randomOffset.x, 0, randomOffset.y);

        GameObject newIngredient = Instantiate(
            mapping.visualReferencePrefab,
            spawnPosition,
            pizzaSpawner.currentPizzaBase.transform.rotation,
            pizzaSpawner.currentPizzaBase.transform
        );

        newIngredient.transform.localScale = Vector3.one * mapping.spawnScale;
        pizzaIngredients[imageName].Add(newIngredient);
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
        foreach (var ingredientList in pizzaIngredients.Values)
        {
            foreach (var ingredient in ingredientList)
            {
                if (ingredient != null)
                {
                    Destroy(ingredient);
                }
            }
        }

        pizzaIngredients.Clear();
        lastPlacementTimes.Clear();
        Debug.Log("Todos los ingredientes de la pizza han sido eliminados");
    }

    private void HandleNewPizza()
    {
        foreach (var trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                CheckPizzaProximity(trackedImage);
            }
        }
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

        ClearPizzaIngredients();
    }
}