
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class imageTracker : MonoBehaviour
{
    [System.Serializable]
    public class IngredientMapping
    {
        public string imageName;
        public GameObject ingredientPrefab;
        public float spawnScale = 1.0f;
    }

    [Header("AR Components")]
    private ARTrackedImageManager trackedImageManager;

    [Header("Ingredient Settings")]
    [SerializeField] private List<IngredientMapping> ingredientMappings = new List<IngredientMapping>();

    // Diccionario para mantener un seguimiento de los objetos instanciados
    private readonly Dictionary<string, GameObject> spawnedIngredients = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Buscar ARTrackedImageManager en la escena
        GameObject xrOrigin = GameObject.Find("XR Origin (Mobile AR)");

        if (xrOrigin != null)
        {
            trackedImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();

            // Si no hay un ARTrackedImageManager, añadirlo
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
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
        else
        {
            Debug.LogError("ARTrackedImageManager no está disponible");
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Procesar imágenes añadidas
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        // Actualizar imágenes que han cambiado
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            // Solo actualizar la posición si el estado de seguimiento es bueno
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateIngredientPosition(trackedImage);
            }
            else
            {
                // Ocultar el ingrediente cuando se pierde el seguimiento
                if (spawnedIngredients.TryGetValue(trackedImage.referenceImage.name, out GameObject ingredient))
                {
                    ingredient.SetActive(false);
                }
            }
        }

        // Proceso para imágenes eliminadas
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            // Ocultar los ingredientes asociados
            if (spawnedIngredients.TryGetValue(trackedImage.referenceImage.name, out GameObject ingredient))
            {
                ingredient.SetActive(false);
            }
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // Buscar la configuración correspondiente a esta imagen
        IngredientMapping mapping = GetMappingByName(imageName);

        if (mapping != null)
        {
            // Verificar si ya existe un objeto para esta imagen
            if (spawnedIngredients.TryGetValue(imageName, out GameObject ingredient))
            {
                // Si existe, activarlo y actualizar su posición
                ingredient.SetActive(true);
                ingredient.transform.position = trackedImage.transform.position;
                ingredient.transform.rotation = trackedImage.transform.rotation;
            }
            else
            {
                // Si no existe, crear uno nuevo
                GameObject newIngredient = Instantiate(mapping.ingredientPrefab,
                                                      trackedImage.transform.position,
                                                      trackedImage.transform.rotation);

                // Ajustar escala
                newIngredient.transform.localScale = Vector3.one * mapping.spawnScale;

                // Agregar al diccionario
                spawnedIngredients.Add(imageName, newIngredient);

                Debug.Log($"Ingrediente detectado: {imageName}");
            }
        }
        else
        {
            Debug.LogWarning($"Imagen detectada '{imageName}' sin configuración de ingrediente");
        }
    }

    private void UpdateIngredientPosition(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedIngredients.TryGetValue(imageName, out GameObject ingredient) && ingredient.activeSelf)
        {
            // Suavizar la actualización de posición para evitar el temblor
            Vector3 targetPosition = Vector3.Lerp(ingredient.transform.position,
                                                trackedImage.transform.position,
                                                Time.deltaTime * 8);

            Quaternion targetRotation = Quaternion.Slerp(ingredient.transform.rotation,
                                                       trackedImage.transform.rotation,
                                                       Time.deltaTime * 8);

            ingredient.transform.position = targetPosition;
            ingredient.transform.rotation = targetRotation;

            // Asegurarse de que esté visible
            ingredient.SetActive(true);
        }
    }

    private IngredientMapping GetMappingByName(string imageName)
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
}