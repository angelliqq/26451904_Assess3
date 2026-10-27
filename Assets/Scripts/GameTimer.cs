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
    }
    
    // --------------------------------- Starting the countdown
    void Start()
    {
        StartCoroutine(Countdown());
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
    }
}
