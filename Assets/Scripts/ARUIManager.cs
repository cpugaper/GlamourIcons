using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARUIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button returnButton;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("ARVolume", 1f);

        volumeSlider.onValueChanged.AddListener(UpdateVolume);
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        homeButton.onClick.AddListener(ReturnToMainMenu);
        returnButton.onClick.AddListener(ToggleSettingsPanel);

        settingsPanel.SetActive(false);
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);

            // Pause game
            Time.timeScale = settingsPanel.activeSelf ? 0 : 1;
        }
    }

    private void UpdateVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("ARVolume", value);
    }


    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        LoadingScreenManager.LoadScene("MainMenu");
    }
}
