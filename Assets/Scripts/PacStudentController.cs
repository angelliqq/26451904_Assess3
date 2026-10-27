using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PacStudentController : MonoBehaviour
{

    // --------------------------------- directions enum
    enum Direction
    {
        None,
        Up,
        Left,
        Down,
        Right,
    }
    
    // --------------------------------- timemap + pellet allocation
    [SerializeField] private Tilemap[] tilemap;
    [SerializeField] private Transform[] pellets;
    
    
    // --------------------------------- teleporter stuff!
    [SerializeField] private Transform TPRight;
    [SerializeField] private Transform TPLeft;

    private Vector2Int leftEnd;
    private Vector2Int rightEnd;
    
    
    // --------------------------------- pacstudent speed
    [SerializeField] private float moveSpeed = 5f;
    
    
    // --------------------------------- pacstudent walk cycle
    private Animator animator;

    
    // --------------------------------- pacstudent audio
    [SerializeField] private AudioSource movement;
    [SerializeField] private AudioClip movingClip;
    [SerializeField] private AudioClip pelletClip;

    
    // --------------------------------- walking trail
    [SerializeField] private ParticleSystem trail;
    
    
    // --------------------------------- movement mechanics
    Direction lastInput =  Direction.None;
    Direction currentInput = Direction.None;

    
    // --------------------------------- tile places + lerping (Movement mechanics)
    private Vector2Int currentTile;
    private Vector2Int targetTile;
    Vector2 targetPosition;
    private bool isLerping;

    
    // --------------------------------- collision sounds
    [SerializeField] private AudioSource bump;
    [SerializeField] private AudioClip bumpClip;
    [SerializeField] private ParticleSystem bumpEffect;
    private bool wasMoving;

    
    // --------------------------------- Division
    private Grid grid;
    readonly Dictionary<Vector2Int, GameObject> pelletsByCell = new Dictionary<Vector2Int, GameObject>();
    
    
    // --------------------------------- General score
    [SerializeField] private TMP_Text scoreTxt;
    private int score;
    
    
    // --------------------------------- Eating a cherry
    [SerializeField] private AudioClip cherryClip;
    
    
    // --------------------------------- Handles all the ghost stuff + power pellet function
    [SerializeField] private GhostManager ghostManager;
    
    
    // --------------------------------- Player icon + effect + pacstudent dead
    [SerializeField] private GameObject[] lifeIcon;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private string deathState = "Death";
    [SerializeField] private float deathDuration = 1.5f;
    
    private int lives = 3;
    private bool isDying;
    private Vector2Int startTile;
    public bool IsDying => isDying;
    
    // --------------------------------- game over section
    [SerializeField] private GameOver gameOver;
    public int Score => score;
    
    
    // --------------------------------- start
    void Start()
    {
        animator = GetComponent<Animator>();

        grid = tilemap[0].layoutGrid;
        
        PelletLookup();
        
        currentTile = WorldToCell(transform.position);
        targetTile = currentTile;
        UpdateLives();
        transform.position = CellToWorld(currentTile);
        targetPosition = transform.position;
        
        // teleporters
        leftEnd = WorldToCell(TPLeft.position);
        rightEnd = WorldToCell(TPRight.position);

        UpdateScore();

    }
    
    // --------------------------------- Update // gameplay
    void Update()
    {
        if (isDying) return;
        ReadInput();
        if (isLerping)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (((Vector2)transform.position - targetPosition).sqrMagnitude < 0.000001f)
            {
                transform.position = targetPosition;
                currentTile = targetTile;
                isLerping = false;
                EatPellet();
                CheckTeleport();
            }
        }

        if (!isLerping)
        {
            if (TryMove(lastInput))
            {
                currentInput = lastInput;
            }
            else if (!TryMove(currentInput))
            {
                if (wasMoving)
                {
                    wasMoving = false;
                    WallBump();
                }
                StopSound();
                SetAnimation(false);
            }
        }

        if (trail != null)
        {
            if (isLerping && !trail.isEmitting) trail.Play();
            else if (!isLerping && trail.isEmitting) trail.Stop();
        }
    }

    // --------------------------------- Handle for all the pellets
    void PelletLookup()
    {
        pelletsByCell.Clear();

        foreach (Transform pellet in pellets)
        {
            if (pellet == null) continue;

                foreach (Transform t in pellet.GetComponentsInChildren<Transform>())
                {
                    if (t.childCount > 0) continue;
                    pelletsByCell[WorldToCell(t.position)] = t.gameObject;
                }
        }
    }

    // --------------------------------- reading player input
    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Direction.Up;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Direction.Left;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Direction.Down;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Direction.Right;
    }

    // --------------------------------- Handles movement w the grid
    bool TryMove(Direction dir)
    {
        if (dir == Direction.None) return false;
        Vector2Int next = currentTile + GetOffSet(dir);
        
        if (!IsWalkable(next)) return false;
        
        targetTile = next;
        targetPosition = CellToWorld(next);
        isLerping = true;
        wasMoving = true;
        if (bumpEffect != null) bumpEffect.Clear();

        bool eat = pelletsByCell.TryGetValue(next, out GameObject pellet) && pellet != null;
        
        ApplyFacing(dir);
        SetAnimation(true);
        PlaySound(eat);
        return true;
    }

    // --------------------------------- checks if tile is walkable
    bool IsWalkable(Vector2Int pos)
    {
        if (pos.x < leftEnd.x || pos.x > rightEnd.x) return false;
        Vector3 world = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        
        foreach (Tilemap map in tilemap)
        {
            if (map == null) continue;
            if (map.GetColliderType(map.WorldToCell(world)) != Tile.ColliderType.None) return false;
        }

        return true;
    }

    // --------------------------------- Cell stuff for the movement function + handles grid for tilemap
    Vector2Int WorldToCell(Vector2 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    Vector2 CellToWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
    }

    static Vector2Int GetOffSet(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
        }
        return Vector2Int.zero;
    }
    
    Direction facing = Direction.None;

    // --------------------------------- Animations depending on player input
    void ApplyFacing(Direction dir)
    {
        if (animator == null || dir == facing) return;
        facing = dir;

        switch (dir)
        {
            case Direction.Up: animator.Play("WalkDown"); break;
            case Direction.Down: animator.Play("WalkUp"); break;
            case Direction.Left: animator.Play("WalkLeft"); break;
            case Direction.Right: animator.Play("WalkRight"); break;
        }
    }
    
    // --------------------------------- Movement + pellet eating sounds
    void PlaySound(bool eating)
    {
        if (movement == null) return;
        AudioClip moving = eating ? pelletClip : movingClip;
        movement.loop = true;

        if (movement.clip != moving || !movement.isPlaying)
        {
            movement.clip = moving;
            movement.Play();
        }
    }

    // --------------------------------- Stops movement sound if player collides
    void StopSound()
    {
        if (movement != null && movement.isPlaying) movement.Stop();
    }
    
    // --------------------------------- pausing animation
    void SetAnimation(bool on)
    {
        if (animator != null) animator.speed = on ?  1f : 0f;
    }
    
    // --------------------------------- Actually handles collisions
    void OnTriggerEnter2D(Collider2D other)
    {

        if (!enabled) return;
        
        // ghost scared states
        if (other.CompareTag("Ghost"))
        {
            if (isDying) return;
            
            Animator ghostAnimator  = other.GetComponentInParent<Animator>();

            if (ghostManager != null && ghostManager != null)
            {
                if (ghostManager.IsGhostDead(ghostAnimator)) return;

                if (ghostManager.IsScared)
                {
                    ghostManager.EatGhost(ghostAnimator);
                    AddScore(300);
                    return;
                }
            }

            StartCoroutine(Die());
            return;
        }
        
        // replacement for cherry tag
        if (other.GetComponent<CherryScore>() != null)
        {
            EatCherry(other.gameObject);
            return;
        }
        
        // wall collision
        if (other.GetComponent<Tilemap>() == null && !IsWalkable(targetTile))
        {
            isLerping = false;
            targetTile = currentTile;
            transform.position = CellToWorld(currentTile);
            StopSound();
            SetAnimation(false);
        }
    }

    // --------------------------------- Bump effect + sound
    void WallBump()
    {
        Vector2Int offset = GetOffSet(currentInput);
        Vector2 halfSize = grid.cellSize * 0.5f;
        Vector2 point = CellToWorld(currentTile) + new Vector2(offset.x * halfSize.x,  offset.y * halfSize.y);

        if (bumpEffect != null)
        {
            bumpEffect.transform.position = point;
            bumpEffect.Emit(8);
        }

        if (bump != null && bumpClip != null) bump.PlayOneShot(bumpClip);
    }
    
    // --------------------------------- Checks if the player lands on teleport
    void CheckTeleport()
    {
        if (currentTile.x == leftEnd.x) Teleport(new  Vector2Int(rightEnd.x, currentTile.y));
        else if (currentTile.x == rightEnd.x) Teleport(new  Vector2Int(leftEnd.x, currentTile.y));
    }

    // --------------------------------- teleport function
    void Teleport(Vector2Int destination)
    {
        currentTile = destination;
        targetTile = destination;
        transform.position = CellToWorld(destination);
        targetPosition = transform.position;
        if (trail != null) trail.Clear();
    }
    
    // --------------------------------- Overall score handling system
    void AddScore(int points)
    {
        score += points;
        UpdateScore();
    }

    // --------------------------------- Updates score into the 6 digits
    void UpdateScore()
    {
        if (scoreTxt != null) scoreTxt.text = score.ToString("D6");
    }

    // --------------------------------- Eat pellet functions
    void EatPellet()
    {
        if (pelletsByCell.TryGetValue(currentTile, out GameObject pellet))
        {
            pelletsByCell.Remove(currentTile);
            
            if (pellet != null)
            {
                bool isPower = pellet.name.StartsWith("PowerPellet");
                Destroy(pellet);

                if (isPower)
                {
                    AddScore(50);
                    if (ghostManager != null) ghostManager.StartScared();
                }
                else
                {
                    AddScore(10);
                }
            }
            // if all pellets are eaten, finish game
            if (pelletsByCell.Count == 0 && gameOver != null) gameOver.EndGame();
        }
    }

    // --------------------------------- Eats cherry
    void EatCherry(GameObject cherry)
    {
        Destroy(cherry);
        AddScore(100);
        if (bump != null && cherryClip != null) bump.PlayOneShot(cherryClip);
    }
    
    // --------------------------------- death section
    IEnumerator Die()
    {
        isDying =  true;
        isLerping = false;
        
        StopSound();
        if (trail != null) trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        lives--;
        UpdateLives();

        if (deathEffect != null)
        {
            deathEffect.transform.position = transform.position;
            deathEffect.Emit(8);
        }
        
        if (bump != null && deathClip != null) bump.PlayOneShot(deathClip);

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(deathState);
        }
        yield return new WaitForSeconds(deathDuration);
        if (lives > 0) Respawn();
        else if (gameOver != null) gameOver.EndGame();
    }

    // --------------------------------- life icon update
    void UpdateLives()
    {
        for (int i = 0; i < lifeIcon.Length; i++)
        {
            if (lifeIcon[i] != null) lifeIcon[i].SetActive(i < lives);
        }
    }

    // --------------------------------- Respawn mechanic for pacstudent
    void Respawn()
    {
        currentTile = startTile;
        targetTile = startTile;
        transform.position = CellToWorld(startTile);
        targetPosition = transform.position;

        lastInput = Direction.None;
        currentInput = Direction.None;
        wasMoving = false;

        facing = Direction.None;
        ApplyFacing(Direction.Right);
        if (animator != null) animator.Update(0f);
        SetAnimation(false);

        if (ghostManager != null) ghostManager.ResetGhosts();
        isDying = false;
    }

    // --------------------------------- freezes whole game
    public void Freeze()
    {
        isLerping = false;
        StopSound();
        SetAnimation(false);
        if (trail != null) trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        enabled = false;
    }
}
