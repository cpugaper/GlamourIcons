using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class IngredientManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float ingredientSpawnHeight = 0.02f;
    [SerializeField] private float detectionRadius = 0.15f;

    private ARTrackedImage trackedImage;
    private GameObject ingredientInstance;
    private PizzaSpawner pizzaSpawner;
    private ImageTracker imageTracker;

    private void Awake()
    {
        trackedImage = GetComponent<ARTrackedImage>();
        if (pizzaSpawner == null)
        {
            pizzaSpawner = FindAnyObjectByType<PizzaSpawner>();
            if (pizzaSpawner == null)
            {
                Debug.LogError("No se encontró PizzaSpawner en la escena");
            }
        }

        if (imageTracker == null)
        {
            imageTracker = FindAnyObjectByType<ImageTracker>();
            if (imageTracker == null)
            {
                Debug.LogError("No se encontró ImageTracker en la escena");
            }
        }
    }

    private void Update()
    {
        if (trackedImage == null || pizzaSpawner == null || pizzaSpawner.currentPizzaBase == null)
        {
            DeactivateIngredient();
            return;
        }

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            DeactivateIngredient();
            return;
        }

        CheckPizzaProximity();
    }

    private void CheckPizzaProximity()
    {
        if (pizzaSpawner.currentPizzaBase == null)
        {
            DeactivateIngredient();
            return;
        }

        float distance = Vector3.Distance(transform.position, pizzaSpawner.currentPizzaBase.transform.position);

        if (distance <= detectionRadius)
        {
            if (ingredientInstance == null || !ingredientInstance.activeSelf)
            {
                SpawnIngredientOnPizza();
            }
            else
            {
                UpdateIngredientPosition();
            }
        }
        else
        {
            DeactivateIngredient();
        }
    }

    private void SpawnIngredientOnPizza()
    {
        if (imageTracker == null || trackedImage.referenceImage == null) return;

        var mapping = imageTracker.GetMappingByName(trackedImage.referenceImage.name);

        if (mapping == null || mapping.ingredientPrefab == null) return;

        Vector3 spawnPosition = pizzaSpawner.currentPizzaBase.transform.position +
            (Vector3.up * ingredientSpawnHeight);

        ingredientInstance = Instantiate(
            mapping.ingredientPrefab,
            spawnPosition,
            pizzaSpawner.currentPizzaBase.transform.rotation,
            pizzaSpawner.currentPizzaBase.transform
        );

        ingredientInstance.transform.localScale = Vector3.one * mapping.spawnScale;
        ingredientInstance.SetActive(true);
    }

    private void UpdateIngredientPosition()
    {
        if (ingredientInstance == null || pizzaSpawner.currentPizzaBase == null) return;

        ingredientInstance.transform.position = pizzaSpawner.currentPizzaBase.transform.position +
            (Vector3.up * ingredientSpawnHeight);
        ingredientInstance.transform.rotation = pizzaSpawner.currentPizzaBase.transform.rotation;
    }

    private void DeactivateIngredient()
    {
        if (ingredientInstance != null && ingredientInstance.activeSelf)
        {
            ingredientInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (ingredientInstance != null)
        {
            Destroy(ingredientInstance);
        }
    }
}