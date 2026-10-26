using TMPro;
using UnityEngine;
using System.Collections;

public class GhostManager : MonoBehaviour
{

    [SerializeField] private string scared = "Scared Left";
    [SerializeField] private string recoveringState= "Right recover";
    [SerializeField] private string dead = "Dead";
    
    
    [SerializeField] private Animator[] ghosts;

    [SerializeField] private GameObject timerObject;
    [SerializeField] private TMP_Text timerTxt;
    
    [SerializeField] private AudioSource bgMusic;
    [SerializeField] private AudioSource scaredAudio;
    [SerializeField] private AudioClip scaredMusic;

    [SerializeField] private float scaredTime = 10f;
    [SerializeField] private float recoveryTime = 3f;
    
    private Coroutine scaredState;
    private int[] savedStates;
    
    
    void Start()
    {
        if (timerObject != null) timerObject.SetActive(false);
    }

    public void StartScared()
    {
        if (scaredState == null) SaveStates();
        else StopCoroutine(scaredState);
            
        scaredState = StartCoroutine(ScaredState());
    }

    IEnumerator ScaredState()
    {
        SetGhostState(scared);
        StartScaredMusic();
        if (timerObject != null) timerObject.SetActive(true);

        bool recovering = false;
        float remaining = scaredTime;

        while (remaining > 0f)
        {
            if (timerTxt != null) timerTxt.text = Mathf.CeilToInt(remaining).ToString();
            if (!recovering && remaining <= recoveryTime)
            {
                recovering = true;
                SetGhostState(recoveringState);
            }
            remaining -= Time.deltaTime;
            yield return null;
        }
        EndScared();
    }

    void EndScared()
    {
        RestoreStates();
        EndScaredMusic();
        if (timerObject  != null) timerObject.SetActive(false);
        scaredState = null;
    }

    void SetGhostState(string state)
    {
        foreach (Animator ghost in ghosts)
        {
            if (ghost == null) continue;
            if (ghost.GetCurrentAnimatorStateInfo(0).IsName(dead)) continue;
            ghost.Play(state);
        }
    }

    void SaveStates()
    {
        savedStates = new int[ghosts.Length];

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null) savedStates[i] = ghosts[i].GetCurrentAnimatorStateInfo(0).shortNameHash;
        }
    }

    void RestoreStates()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] == null) continue;
            if (ghosts[i].GetCurrentAnimatorStateInfo(0).IsName(dead)) continue;
            ghosts[i].Play(savedStates[i]);
        }
    }

    void StartScaredMusic()
    {
        if (bgMusic != null) bgMusic.mute = true;

        if (scaredAudio != null && scaredMusic != null && !scaredAudio.isPlaying)
        {
            scaredAudio.clip = scaredMusic;
            scaredAudio.loop = true;
            scaredAudio.Play();
        }
    }

    void EndScaredMusic()
    {
        if (scaredAudio != null) scaredAudio.Stop();
        if (bgMusic != null) bgMusic.mute = false;
    }
}
