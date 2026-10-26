using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PacStudentController : MonoBehaviour
{

    enum Direction
    {
        None,
        Up,
        Left,
        Down,
        Right,
    }
    
    [SerializeField] private Tilemap[] tilemap;
    [SerializeField] private Transform[] pellets;
    
    
    private float moveSpeed = 5f;
    
    private Animator animator;

    [SerializeField] private AudioSource movement;
    [SerializeField] private AudioClip movingClip;
    [SerializeField] private AudioClip pelletClip;

    [SerializeField] private ParticleSystem trail;
    
    Direction lastInput =  Direction.None;
    Direction currentInput = Direction.None;

    private Vector2Int currentTile;
    private Vector2Int targetTile;
    Vector2 targetPosition;
    private bool isLerping;

    private Grid grid;
    readonly Dictionary<Vector2Int, GameObject> pelletsByCell = new Dictionary<Vector2Int, GameObject>();
    
    void Start()
    {
        animator = GetComponent<Animator>();

        grid = tilemap[0].layoutGrid;
        
        PelletLookup();
        
        currentTile = WorldToCell(transform.position);
        targetTile = currentTile;
        transform.position = CellToWorld(currentTile);
        targetPosition = transform.position;
        
    }
    
    void Update()
    {
        ReadInput();

        if (isLerping)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (((Vector2)transform.position - targetPosition).sqrMagnitude < 0.000001f)
            {
                transform.position = targetPosition;
                currentTile = targetTile;
                isLerping = false;
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

    void PelletLookup()
    {
        pelletsByCell.Clear();

        foreach (Transform pellet in pellets)
        {
            if (pellet == null) continue;

                foreach (Transform t in pellet.GetComponentsInChildren<Transform>())
                {
                    if (t == pellet && pellet.childCount > 0) continue;
                    pelletsByCell[WorldToCell(t.position)] = t.gameObject;
                }
        }
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Direction.Up;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Direction.Left;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Direction.Down;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Direction.Right;
    }

    bool TryMove(Direction dir)
    {
        if (dir == Direction.None) return false;
        Vector2Int next = currentTile + GetOffSet(dir);
        
        if (!IsWalkable(next)) return false;
        
        targetTile = next;
        targetPosition = CellToWorld(next);
        isLerping = true;

        bool eat = pelletsByCell.TryGetValue(next, out GameObject pellet) && pellet != null;
        
        ApplyFacing(dir);
        SetAnimation(true);
        PlaySound(eat);
        return true;
    }

    // checks if tile is walkable
    bool IsWalkable(Vector2Int pos)
    {
        Vector3 world = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        
        foreach (Tilemap map in tilemap)
        {
            if (map == null) continue;
            if (map.GetColliderType(map.WorldToCell(world)) != Tile.ColliderType.None) return false;
        }

        return true;
    }

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
    
    // sfx
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

    // stopping sfx
    void StopSound()
    {
        if (movement != null && movement.isPlaying) movement.Stop();
    }
    
    // pausing animation
    void SetAnimation(bool on)
    {
        if (animator != null) animator.speed = on ?  1f : 0f;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Tilemap>() == null) return;
        if (isLerping && !IsWalkable(targetTile))
        {
            isLerping = false;
            targetTile = currentTile;
            transform.position = CellToWorld(currentTile);
            StopSound();
            SetAnimation(false);
        }
    }
}
