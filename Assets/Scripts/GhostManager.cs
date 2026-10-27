using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostManager : MonoBehaviour
{

    // PS im using headers to sort out eveything since theres too much stuff!
    
    // --------------------------------- ANIMATIONS
    [SerializeField] private string scared = "Scared Left";
    [SerializeField] private string recoveringState= "Right recover";
    [SerializeField] private string dead = "Dead";
    
    [SerializeField] private Animator[] ghosts;
    
    // --------------------------------- Scared timer
    [SerializeField] private GameObject timerObject;
    [SerializeField] private TMP_Text timerTxt;
    
    // --------------------------------- Scared music
    [SerializeField] private AudioSource bgMusic;
    [SerializeField] private AudioSource scaredAudio;
    [SerializeField] private AudioClip scaredMusic;
    [SerializeField] private AudioClip deadMusic;
    [SerializeField] private AudioClip ghostEatClip;

    // --------------------------------- Timers
    [SerializeField] private float scaredTime = 10f;
    [SerializeField] private float recoveryTime = 3f;
    [SerializeField] private float deadTime = 3f;
    
    // --------------------------------- states
    private Coroutine scaredState;
    private bool scaredActive;
    private float remaining;
    private int[] savedStates;
    private Vector3[] startPositions;
    private readonly HashSet<Animator> deadGosts = new HashSet<Animator>();
    public bool IsScared => scaredActive;


    // --------------------------------- is ghost dead?
    public bool IsGhostDead(Animator ghost)
    {
        return deadGosts.Contains(ghost);
    }
    
    // --------------------------------- start in general
    void Start()
    {
        SetTimerVisible(false);
        
        startPositions = new Vector3[ghosts.Length];
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null) startPositions[i] = ghosts[i].transform.position;
        }
    }

    // --------------------------------- Starting scared state
    public void StartScared()
    {
        if (!scaredActive) SaveStates();
        else StopCoroutine(scaredState);
            
        scaredState = StartCoroutine(ScaredState());
    }

    // --------------------------------- scared state in general
    IEnumerator ScaredState()
    {

        scaredActive = true;
        remaining = scaredTime;
        
        SetTimerVisible(true);
        SetGhostState(scared);
        UpdateMusic();

        bool recovering = false;

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

    // --------------------------------- finished scared state
    void EndScared()
    {

        scaredActive = false;
        scaredState = null;
        remaining = 0f;
        
        RestoreStates();
        SetTimerVisible(false);
        UpdateMusic();
    }

    // --------------------------------- applying the animated state
    void SetGhostState(string state)
    {
        foreach (Animator ghost in ghosts)
        {
            if (ghost == null) continue;
            if (deadGosts.Contains(ghost)) continue;
            ghost.Play(state);
        }
    }

    // --------------------------------- saved the states
    void SaveStates()
    {
        savedStates = new int[ghosts.Length];

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null) savedStates[i] = ghosts[i].GetCurrentAnimatorStateInfo(0).shortNameHash;
        }
    }

    // --------------------------------- Restores states 
    void RestoreStates()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] == null) continue;
            if (deadGosts.Contains(ghosts[i])) continue;
            ghosts[i].Play(savedStates[i]);
        }
    }
    
    // --------------------------------- replaced music stuff with one method
    // --------------------------------- update music!

    void UpdateMusic()
    {
        AudioClip wanted = null;
        
        if (deadGosts.Count > 0) wanted = deadMusic;
        else if (scaredActive) wanted = scaredMusic;
        
        if (bgMusic != null) bgMusic.mute = wanted != null;
        if (scaredAudio == null) return;

        if (wanted == null)
        {
            scaredAudio.Stop();
            return;
        }

        if (scaredAudio.clip != wanted || !scaredAudio.isPlaying)
        {
            scaredAudio.clip = wanted;
            scaredAudio.loop = true;
            scaredAudio.Play();
        }
    }

    // --------------------------------- resets ghosts to normal
    public void ResetGhosts()
    {
        StopAllCoroutines();
        scaredState = null;
        scaredActive = false;
        remaining = 0f;
        deadGosts.Clear();
        
        SetTimerVisible(false);
        UpdateMusic();

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] == null) continue;
            ghosts[i].transform.position = startPositions[i];
            ghosts[i].Rebind();
            ghosts[i].Update(0f);
        }
    }

    // --------------------------------- ghost eaten + sound clip when eaten
    public void EatGhost(Animator ghost)
    {
        if (ghost == null || deadGosts.Contains(ghost)) return;
        deadGosts.Add(ghost);
        ghost.Play("Dead");

        if (ghostEatClip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(ghostEatClip, Camera.main.transform.position);
        }
        UpdateMusic();
        StartCoroutine(ReviveRoutine(ghost));
    }

    // --------------------------------- respawn ghost routine
    IEnumerator ReviveRoutine(Animator ghost)
    {
        yield return new WaitForSeconds(deadTime);
        
        deadGosts.Remove(ghost);

        if (!scaredActive)
        {
            ghost.Rebind();
            ghost.Update(0f);
        }
        else if (remaining > recoveryTime)
        {
            ghost.Play("Scared");
        }
        else
        {
            ghost.Play(recoveringState);
        }
        UpdateMusic();
    }

    // scared state timer
    void SetTimerVisible(bool visible)
    {
        if (timerObject != null) timerObject.SetActive(visible);
        if (timerTxt != null) timerTxt.gameObject.SetActive(visible);
    }
}
