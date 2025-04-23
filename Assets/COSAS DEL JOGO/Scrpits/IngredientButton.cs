using UnityEngine;
using UnityEngine.UI;

public class IngredientButton : MonoBehaviour
{
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private Button button;
    [SerializeField] private PizzaSpawner pizzaSpawner;

    private static IngredientButton currentlySelectedButton;

    private bool isSelected = false;
    private Image buttonImage;
    private Color normalColor;
    [SerializeField] private Color selectedColor = Color.green;

    private void Awake()
    {
        buttonImage = button.GetComponent<Image>();
        normalColor = buttonImage.color;

        button.onClick.AddListener(ToggleSelection);

        if (pizzaSpawner != null)
        {
            pizzaSpawner.OnPizzaDestroyed += OnPizzaDestroyed;
        }
    }

    private void OnDestroy()
    {
        if (pizzaSpawner != null)
        {
            pizzaSpawner.OnPizzaDestroyed -= OnPizzaDestroyed;
        }

        if (currentlySelectedButton == this)
        {
            currentlySelectedButton = null;
        }
    }

    private void ToggleSelection()
    {
        if (pizzaSpawner == null || pizzaSpawner.currentPizzaBase == null) return;

        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            currentlySelectedButton.Deselect();
        }

        isSelected = !isSelected;

        if (isSelected)
        {
            currentlySelectedButton = this;
            buttonImage.color = selectedColor;
        }
        else
        {
            currentlySelectedButton = null;
            buttonImage.color = normalColor;
        }
    }

    public void Deselect()
    {
        isSelected = false;
        buttonImage.color = normalColor;
    }

    public bool TryPlaceIngredient(Vector3 position, Vector3 normal)
    {
        if (!isSelected || pizzaSpawner == null || pizzaSpawner.currentPizzaBase == null)
            return false;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        GameObject ingredient = Instantiate(
            ingredientPrefab,
            position + normal * 0.005f, 
            rotation,
            pizzaSpawner.currentPizzaBase.transform
        );

        ingredient.transform.localRotation = Quaternion.identity;

        return true;
    }

    private void OnPizzaDestroyed()
    {
        Deselect();
        if (currentlySelectedButton == this)
        {
            currentlySelectedButton = null;
        }
    }
    public bool IsSelected()
    {
        return isSelected;
    }
}