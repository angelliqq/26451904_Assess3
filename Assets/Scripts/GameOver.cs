using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    // --------------------------------- UI
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject gameOverTxt;

    // --------------------------------- Controllers + functions
    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private Behaviour[] ghostBehaviour;
    [SerializeField] private Button[] button;

    // --------------------------------- Return to start scene
    private string startScene = "StartScene";
    private float displayTime = 3f;

    private bool isOver;

    // --------------------------------- if game isnt over dont show text
    void Awake()
    {
        if (gameOverTxt != null) gameOverTxt.SetActive(false);
    }

    // --------------------------------- ends the game
    public void EndGame()
    {
        if (isOver) return;
        isOver = true;
        StartCoroutine(GameOverRoutine());
    }

    // --------------------------------- routine for finished game
    IEnumerator GameOverRoutine()
    {
        if (pacStudent != null) pacStudent.Freeze();

        foreach (Behaviour b in ghostBehaviour)
        {
            if (b != null) b.enabled = false;
        }

        if (gameTimer != null) gameTimer.StopTimer();

        foreach (Button buttons in button)
        {
            if (buttons != null) buttons.interactable = false;
        }

        SaveHighScore();

        if (panel != null) panel.SetActive(true);
        if (gameOverTxt != null) gameOverTxt.SetActive(true);

        yield return new WaitForSeconds(displayTime);
        SceneManager.LoadScene(startScene);
    }

    // --------------------------------- saves score (also everytime its a better one)
    void SaveHighScore()
    {
        if (pacStudent == null || gameTimer == null) return;

        int score = pacStudent.Score;
        float time = gameTimer.Elapsed;

        bool hasSaved = PlayerPrefs.HasKey(HighScore.ScoreKey);
        int bestScore = PlayerPrefs.GetInt(HighScore.ScoreKey, 0);
        float bestTimer = PlayerPrefs.GetFloat(HighScore.TimerKey, 0f);

        bool betterScore = score > bestScore || (hasSaved && score == bestScore && time < bestTimer);

        if (betterScore)
        {
            PlayerPrefs.SetInt(HighScore.ScoreKey, score);
            PlayerPrefs.SetFloat(HighScore.TimerKey, time);
            PlayerPrefs.Save();
        }

    }
}