using System;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

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
    
    // --------------------------------- states
    private Coroutine scaredState;
    private bool scaredActive;
    private float remaining;
    private int[] savedStates;
    private Vector3[] startPositions;
    private readonly HashSet<Animator> deadGosts = new HashSet<Animator>();

    // --------------------------------- ghost movement
    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private Tilemap[] tilemaps;
    [SerializeField] private Transform tpLeft;
    [SerializeField] private Transform tpRight;
    [SerializeField] private Collider2D spawn;
    [SerializeField] private Transform exitTop;
    [SerializeField] private Transform exitBottom;
    [SerializeField] private Transform[] ghost4Route;

    private Grid grid;
    private Vector2Int leftEnd;
    private Vector2Int rightEnd;
    
    // --------------------------------- bool section
    public bool IsScared => scaredActive;
    public bool hasRoute => ghost4Route != null && ghost4Route.Length > 0;
    public bool pacDying => pacStudent != null && pacStudent.IsDying;
    public float pacSpeed => pacStudent != null ? pacStudent.MoveSpeed : 5f;
    public Vector2 pacPosition => pacStudent != null ? (Vector2)pacStudent.transform.position : Vector2.zero;
    
    
    // --------------------------------- is ghost dead?
    public bool IsGhostDead(Animator ghost)
    {
        return deadGosts.Contains(ghost);
    }

    void Awake()
    {
        grid = tilemaps[0].layoutGrid;
        leftEnd = WorldToCell(tpLeft.position);
        rightEnd = WorldToCell(tpRight.position);

        startPositions = new Vector3[ghosts.Length];
        
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null) continue;
            startPositions[i] = CellToWorld(WorldToCell(ghosts[i].transform.position));
            ghosts[i].transform.position = startPositions[i];
        }
    }

    // --------------------------------- start in general
    void Start()
    {
        SetTimerVisible(false);
    }

    // --------------------------------- TILEMAP SETUP --- code from pacstudentcontroller
    Vector2Int WorldToCell(Vector2 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    Vector2 CellToWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
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
            
            //new
            GhostController controller = ghosts[i].GetComponent<GhostController>();
            if (controller != null) controller.ResetGhost();
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
    }

    // --------------------------------- respawn ghost (old rountine)
    public void ReviveGhost(Animator ghost)
    {
        if (ghost == null) return;
        
        deadGosts.Remove(ghost);

        if (!scaredActive)
        {
            ghost.Rebind();
            ghost.Update(0f);
        }
        else if (remaining > recoveryTime)
        {
            ghost.Play(scared);
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

    // --------------------------------- ghost starting pos
    public Vector2 StartPosition(int index)
    {
        return startPositions[index];
    }

    // --------------------------------- exit (yellow lines)
    public Vector2 ExitFor(int ghostNumber)
    {
        Transform exit = (ghostNumber == 1 || ghostNumber == 3) ? exitTop : exitBottom;
        return CellToWorld(WorldToCell(exit.position));
    }

    // --------------------------------- waypoints
    public List<Vector2> RoutePoints()
    {
        List<Vector2> points = new List<Vector2>();
        foreach (Transform t in ghost4Route)
        {
            if (t != null) points.Add(CellToWorld(WorldToCell(t.position)));
        }

        return points;
    }

    // --------------------------------- isWalkable pacstudent but for ghosts
    public bool IsGhostWalkable(Vector2Int cell)
    {
        if (cell.x < leftEnd.x || cell.x > rightEnd.x) return false;
        Vector3 world = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        
        foreach (Tilemap map in tilemaps)
        {
            if (map == null) continue;
            if (map.GetColliderType(map.WorldToCell(world)) != Tile.ColliderType.None) return false;
        }

        return true;
    }
}
