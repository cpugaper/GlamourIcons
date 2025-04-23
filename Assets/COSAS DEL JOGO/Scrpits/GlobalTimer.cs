using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TimerGlobal : MonoBehaviour
{
    public float totalTime = 60f; 
    public TMP_Text timerText;


    private float timeLeft;

    void Start()
    {
        timeLeft = totalTime;
    }

    void Update()
    {

        timeLeft -= Time.deltaTime;

       
        if (timeLeft < 0)
            timeLeft = 0;

        
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        
        if (timeLeft <= 0)
        {
            SceneManager.LoadScene("Results");
        }
    }
}
