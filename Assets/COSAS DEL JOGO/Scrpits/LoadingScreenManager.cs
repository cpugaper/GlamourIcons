using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private Image loadingIcon;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float minLoadTime = 2f;

    private static string targetSceneName;
    private void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        float loadStartTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            loadingIcon.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

            if (operation.progress >= 0.9f && (Time.time - loadStartTime) >= minLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public static void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
