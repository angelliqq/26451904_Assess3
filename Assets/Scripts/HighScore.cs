using TMPro;
using UnityEngine;

public class HighScore : MonoBehaviour
{
    
    // --------------------------------- score + timer keys
    
    public const string ScoreKey = "Score";
    public const string TimerKey = "Time";
    
    // --------------------------------- text fields
    
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text timerTxt;
    
    
    // --------------------------------- displays score
    void Start()
    {
        int score = PlayerPrefs.GetInt(ScoreKey, 0);
        float timer = PlayerPrefs.GetFloat(TimerKey, 0f);
        
        if (scoreTxt != null) scoreTxt.text = score.ToString("D6");
        if (timerTxt != null) timerTxt.text = FormatTime(timer);
    }

    // --------------------------------- format for the timer!
    public static string FormatTime(float seconds)
    {       
        int minutes = (int)(seconds / 60f);
        int sec = (int)(seconds % 60f);
        int centiseconds = (int)((seconds * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}:{centiseconds:00}";
    }
}
