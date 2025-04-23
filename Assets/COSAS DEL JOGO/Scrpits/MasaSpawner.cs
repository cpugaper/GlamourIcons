using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class PizzaSpawner : MonoBehaviour
{
    public event System.Action OnPizzaDestroyed;

    [Header("AR Components")]
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    [Header("Pizza Settings")]
    [SerializeField] private GameObject pizzaBasePrefab;
    [SerializeField] private float minPlaneSize = 0.4f;
    [SerializeField] private float pizzaLifetime = 60f;
    [SerializeField] private float pizzaScale = 0.2f;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Order System")]
    [SerializeField] private OrderManager orderManager;

    [Header("Ingredient System")]
    [SerializeField] private LayerMask pizzaLayerMask;

    public GameObject currentPizzaBase;
    private bool canSpawnPizza = true;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private float remainingTime;

    private void Awake()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin (Mobile AR)");

        if (xrOrigin != null)
        {
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
            planeManager = xrOrigin.GetComponent<ARPlaneManager>();
        }
        else
        {
            Debug.LogError("XR Origin (Mobile AR) not found in Scene");
        }
    }

    private void Update()
    {
        if (!canSpawnPizza && currentPizzaBase != null)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (remainingTime <= 0)
            {
                if (orderManager != null)
                {
                    orderManager.OnTimerEnded();
                }
            }
        }
        HandlePizzaTouch();
    }
    private void HandlePizzaTouch()
    {
        if (currentPizzaBase == null) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, pizzaLayerMask))
            {
                if (hit.collider.gameObject == currentPizzaBase)
                {
                    IngredientButton selectedButton = GetSelectedButton();
                    if (selectedButton != null)
                    {
                        selectedButton.TryPlaceIngredient(hit.point, hit.normal);
                    }
                }
            }
        }

    }

    private IngredientButton GetSelectedButton()
    {
        foreach (var button in FindObjectsOfType<IngredientButton>())
        {
            if (button.IsSelected())
            {
                return button;
            }
        }
        return null;
    }

    public void TrySpawnPizza()
    {
        if (!canSpawnPizza || currentPizzaBase != null)
        {
            Debug.Log("Ya hay una masa de pizza en la escena");
            return;
        }

        TryCreatePizzaBase(new Vector2(Screen.width / 2, Screen.height / 2));
    }

    private void TryCreatePizzaBase(Vector2 screenPosition)
    {
        if (!canSpawnPizza || currentPizzaBase != null || raycastManager == null)
            return;

        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = raycastHits[0];
            ARPlane plane = planeManager.GetPlane(hit.trackableId);

            if (IsPlaneValidForPizzaBase(plane))
            {
                Pose hitPose = hit.pose;
                currentPizzaBase = Instantiate(pizzaBasePrefab, hitPose.position, hitPose.rotation);
                currentPizzaBase.transform.up = hit.pose.up;
                currentPizzaBase.transform.localScale = Vector3.one * pizzaScale;

                canSpawnPizza = false;
                remainingTime = pizzaLifetime;
                UpdateTimerDisplay();
                StartCoroutine(DestroyPizzaAfterTime(pizzaLifetime));

                if (orderManager != null)
                {
                    orderManager.ShowOrderForNewPizza();
                }

                DisablePlaneVisualization();

                Debug.Log("Masa de pizza creada. Se destruirá en " + pizzaLifetime + " segundos");
            }
            else
            {
                Debug.Log("Superficie no válida: demasiado pequeña o no horizontal");
            }
        }
        else
        {
            Debug.Log("No se detectó superficie plana");
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Pizza time: {Mathf.CeilToInt(remainingTime)}s";
        }
    }

    private void DisablePlaneVisualization()
    {
        if (planeManager == null) return;

        foreach (var plane in planeManager.trackables)
        {
            var visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (visualizer != null)
            {
                visualizer.enabled = false;
            }

            var collider = plane.GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private bool IsPlaneValidForPizzaBase(ARPlane plane)
    {
        if (plane == null) return false;
        if (plane.alignment != PlaneAlignment.HorizontalUp) return false;
        return plane.size.x >= minPlaneSize && plane.size.y >= minPlaneSize;
    }

    private IEnumerator DestroyPizzaAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (currentPizzaBase != null)
        {
            Destroy(currentPizzaBase);
            currentPizzaBase = null;

            OnPizzaDestroyed?.Invoke();
        }

        canSpawnPizza = true;

        if (timerText != null)
        {
            timerText.text = "";
        }

        Debug.Log("Masa de pizza destruida. Puedes crear una nueva");
    }
}