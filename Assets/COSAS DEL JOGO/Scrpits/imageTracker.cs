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
        public GameObject ingredientPrefab;
        public float spawnScale = 1.0f;
    }

    [Header("AR Components")]
    private ARTrackedImageManager trackedImageManager;

    [Header("Ingredient Settings")]
    [SerializeField] private List<IngredientMapping> ingredientMappings = new List<IngredientMapping>();

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
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage.GetComponent<IngredientManager>() == null)
            {
                trackedImage.gameObject.AddComponent<IngredientManager>();
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
}