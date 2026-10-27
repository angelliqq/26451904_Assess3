using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class GameTimer : MonoBehaviour
{

    // --------------------------------- UI
    
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text countdownTxt;
    
    // --------------------------------- music
    
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip bgClip;
    
    // --------------------------------- pacstudent + ghost movements
    
    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private Behaviour[] ghostBehaviour;

    // --------------------------------- timer interval
    
    private float interval = 1f;
    
    // --------------------------------- Round starting variables
    
    public bool RoundStarted { get; private set; }
    public event System.Action RoundBegins;
    
    // --------------------------------- Stopwatch settings
    [SerializeField] private TMP_Text stopwatch;
    private float elapsedTime;
    private bool running;
    
    public float Elapsed => elapsedTime;

    // --------------------------------- Nothing is active
    void Awake()
    {
        // freeze pacstudent animation
        if (pacStudent != null)
        {
            pacStudent.enabled = false;
            Animator animator = pacStudent.GetComponent<Animator>();
            if (animator != null) animator.speed = 0;
        }
        
        // ghost behaviour disabled
        foreach (Behaviour b in ghostBehaviour)
        {
            if (b != null) b.enabled = false;
        }
        
        // UI is disabled
        if (countdownTxt != null) countdownTxt.gameObject.SetActive(false);
        if (panel != null) panel.SetActive(false);
        
        // stopwatch on default
        elapsedTime = 0f;
        UpdateTimer();
    }
    
    // --------------------------------- Starting the countdown
    void Start()
    {
        StartCoroutine(Countdown());
    }

    // --------------------------------- Time is elapsed during gameplay
    private void Update()
    {
        if (!running) return;
        elapsedTime += Time.deltaTime;
        UpdateTimer();
    }

    // --------------------------------- Countdown function
    IEnumerator Countdown()
    {
        if (panel != null) panel.SetActive(true);
        if (countdownTxt != null) countdownTxt.gameObject.SetActive(true);

        if (music != null && introClip != null)
        {
            music.Stop();
            music.clip = introClip;
            music.loop = false;
            music.Play();
        }

        string[] timer = { "3", "2", "1", "GO!" };
        foreach (string s in timer)
        {
            if (countdownTxt != null) countdownTxt.text = s;
            yield return new WaitForSeconds(interval);
        }

        StartRound();
    }

    // --------------------------------- Round begins /// pacstudent can move and ghost behaviour is enabled
    void StartRound()
    {
        if (countdownTxt != null) countdownTxt.gameObject.SetActive(false);
        if (panel != null) panel.SetActive(false);

        if (music != null)
        {
            music.Stop();
            
            if (bgClip != null)
            {
                music.clip = bgClip;
                music.loop = true;
                music.Play();
            }
        }
        
        if (pacStudent != null) pacStudent.enabled = true;
        foreach (Behaviour b in ghostBehaviour)
        {
            if (b != null) b.enabled = true;
        }
        
        RoundStarted = true;
        RoundBegins?.Invoke();
        running = true;
    }

    // --------------------------------- adding elapsed time thru numbers
    void UpdateTimer()
    {
        if (stopwatch == null) return;
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        int centiseconds = (int)((elapsedTime * 100f) % 100f);
        stopwatch.text = $"{minutes:00}:{seconds:00}:{centiseconds:00}";
    }

    // --------------------------------- stops timer
    public void StopTimer()
    {
        running = false;
    }
}
