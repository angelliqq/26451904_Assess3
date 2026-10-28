using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class GhostController : MonoBehaviour
{
    
    // --------------------------------- ghosts
    
    [SerializeField] private int ghostNumber = 1;
    
    
    // --------------------------------- Tilemap
    
    [SerializeField] private Tilemap[] tileMap;
    [SerializeField] private Transform tpLeft;
    [SerializeField] private Transform tpRight;

    // --------------------------------- pacStudent + ghosts
    
    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private GhostManager ghostManager;

    
    // --------------------------------- movement
    
    [SerializeField] private Transform[] exitPath;
    [SerializeField] private Transform[] route;
    
    // --------------------------------- ghost states
    enum Mode
    {
        Spawn,
        Roam,
        Dead,
    }

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };
    
    // --------------------------------- cells from tilemap
    
    private Mode mode = Mode.Spawn;
    
    private Grid grid;
    private Animator animator;
    
    // --------------------------------- tile places + lerping (Movement mechanics) // taken from pacstudent
    private Vector2 startPosition;
    private Vector2Int leftEnd;
    private Vector2Int rightEnd;
    private Vector2Int currentTile;
    private Vector2Int targetTile;
    Vector2 targetPosition;
    private Vector2Int lastDirection;
    private bool isLerping;

    // --------------------------------- indexes
    
    private int pathIndex;
    private int routeIndex;
    private bool routeNeedsRefresh = true;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        grid = tileMap[0].layoutGrid;
        startPosition = transform.position;
        
        leftEnd = WorldToCell(tpLeft.position);
        rightEnd = WorldToCell(tpRight.position);
    }
    
    // --------------------------------- update
    void Update()
    {
        if (pacStudent != null && pacStudent.IsDying) return;

        switch (mode)
        {
            case Mode.Spawn: UpdateSpawn(); break;
            case Mode.Roam: UpdateRoam(); break;
            case Mode.Dead: UpdateDead(); break;
        }
    }

    // --------------------------------- death for ghost
    public void Die()
    {
        mode = Mode.Dead;
        isLerping = false;
    }

    // --------------------------------- respawns to start
    public void ResetToStart()
    {
        mode = Mode.Spawn;
        pathIndex = 0;
        routeIndex = 0;
        routeNeedsRefresh = true;
        isLerping = false;
        SetPost(startPosition);
    }

    // --------------------------------- ghost speed
    float CurrentSpeed()
    {
        float normal = (pacStudent != null ? pacStudent.MoveSpeed : 5f) * 0.9f;

        if (mode == Mode.Dead) return normal * 0.5f;
        if (ghostManager != null && ghostManager.IsScared) return normal * 0.5f;
        return normal;
    }

    // --------------------------------- updates ghost spawn
    void UpdateSpawn()
    {
        if (exitPath == null || pathIndex >= exitPath.Length)
        {
            FinishSpawn();
            return;
        }
        Vector2 goal = exitPath[pathIndex].position;
        SetPost(Vector2.MoveTowards(transform.position, goal, CurrentSpeed() * Time.deltaTime));
        if (((Vector2)transform.position - goal).sqrMagnitude < 0.000001f)
        {
            SetPost(goal);
            pathIndex++;
        }
    }

    // --------------------------------- finishes spawn load
    void FinishSpawn()
    {
        currentTile = WorldToCell(transform.position);
        SetPost(CellToWorld(currentTile));
        
        bool hasPath = exitPath != null && exitPath.Length > 0;
        Vector2 to = hasPath ? (Vector2)exitPath[exitPath.Length - 1].position : startPosition;
        Vector2 from = hasPath && exitPath.Length > 1 ? (Vector2)exitPath[exitPath.Length - 2].position : startPosition;

        lastDirection = ToAxis(to - from);
        routeNeedsRefresh = true;
        isLerping = false;
        mode = Mode.Roam;
    }

    // --------------------------------- update dead state
    void UpdateDead()
    {
        SetPost(Vector2.MoveTowards(transform.position, startPosition, CurrentSpeed() * Time.deltaTime));
        if (((Vector2)transform.position - startPosition).sqrMagnitude < 0.000001f)
        {
            SetPost(startPosition);
            mode = Mode.Spawn;
            pathIndex = 0;
            isLerping = false;
            
            if (ghostManager != null && animator != null) ghostManager.ReviveGhost(animator);
        }
    }

    // --------------------------------- updating roam
    void UpdateRoam()
    {
        if (isLerping)
        {
            SetPost(Vector2.MoveTowards(transform.position, targetPosition, CurrentSpeed() * Time.deltaTime));
            if (((Vector2)transform.position - targetPosition).sqrMagnitude < 0.000001f)
            {
                SetPost(targetPosition);
                currentTile = targetTile;
                isLerping = false;
            }
        }
        if (!isLerping) ChooseNextMove();
    }

    // --------------------------------- ghost picks next mvoe
    void ChooseNextMove()
    {
        List<Vector2Int> options = GetOptions();
        if (options.Count == 0) return;
        Vector2Int dir = PickDirections(options);
        lastDirection = dir;
        targetTile = currentTile + dir;
        targetPosition = CellToWorld(targetTile);
        isLerping = true;
    }

    // --------------------------------- direction options
    List<Vector2Int> GetOptions()
    {
        List<Vector2Int> options = new List<Vector2Int>();
        Vector2Int reverse = -lastDirection;

        foreach (Vector2Int d in Directions)
        {
            if (d == reverse) continue;
            if (IsWalkable(currentTile + d)) options.Add(d);
        }
        if (options.Count == 0 && IsWalkable(currentTile + reverse)) options.Add(reverse);
        return options;
    }

    // --------------------------------- picking directions
    Vector2Int PickDirections(List<Vector2Int> options)
    {
        bool scared = ghostManager != null && ghostManager.IsScared;
        int behaviour = scared ? 1 : ghostNumber;

        if (ghostNumber == 4 && behaviour != 4) routeNeedsRefresh = true;

        switch (behaviour)
        {
            case 1: return PickByDistance(options, true);
            case 2: return PickByDistance(options, false);
            case 4: return PickRoute(options);
            default: return RandomOf(options);
        }
    }

    // --------------------------------- picks route via distance
    Vector2Int PickByDistance(List<Vector2Int> options, bool further)
    {
        if (pacStudent == null) return RandomOf(options);

        Vector2 pac = pacStudent.transform.position;
        float current = Vector2.Distance(CellToWorld(currentTile), pac);

        List<Vector2Int> matching = new List<Vector2Int>();

        foreach (Vector2Int d in options)
        {
            float next = Vector2.Distance(CellToWorld(currentTile + d), pac);
            if (further && next >= current - 0.0001f) matching.Add(d);
            else if (!further && next <= current + 0.0001f) matching.Add(d);
        }

        return RandomOf(matching.Count > 0 ? matching : options);
    }

    // --------------------------------- picks route
    Vector2Int PickRoute(List<Vector2Int> options)
    {
        if (route == null || route.Length == 0) return RandomOf(options);
        if (routeNeedsRefresh)
        {
            routeIndex = NearestRouteIndex();
            routeNeedsRefresh = false;
        }
        
        Vector2Int goal = WorldToCell(route[routeIndex].position);

        if (currentTile == goal)
        {
            routeIndex = (routeIndex + 1) % route.Length;
            goal = WorldToCell(route[routeIndex].position);
        }
        
        int best = int.MaxValue;
        List<Vector2Int> bestRoute = new List<Vector2Int>();
        
        foreach (Vector2Int d in options)
        {
            Vector2Int next = currentTile + d;
            int dist = Mathf.Abs(next.x - goal.x) + Mathf.Abs(next.y - goal.y);

            if (dist < best)
            {
                best = dist;
                bestRoute.Clear();
                bestRoute.Add(d);
            }
            else if (dist == best)
            {
                bestRoute.Add(d);
            }
        }

        return RandomOf(bestRoute);
    }

    // --------------------------------- nearest route
    int NearestRouteIndex()
    {
        int nearest = 0;
        int best = int.MaxValue;

        for (int i = 0; i < route.Length; i++)
        {
            Vector2Int cell = WorldToCell(route[i].position);
            int dist = Mathf.Abs(cell.x - currentTile.x) + Mathf.Abs(cell.y - currentTile.y);

            if (dist < best)
            {
                best = dist;
                nearest = i;
            }
        }
        return nearest;
    }
    
    // --------------------------------- checks if tile is walkable
    bool IsWalkable(Vector2Int pos)
    {
        if (pos.x <= leftEnd.x || pos.x <= rightEnd.x) return false;
        Vector3 world = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        
        foreach (Tilemap map in tileMap)
        {
            if (map == null) continue;
            if (map.GetColliderType(map.WorldToCell(world)) != Tile.ColliderType.None) return false;
        }
        return true;
    }

    Vector2Int RandomOf(List<Vector2Int> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    static Vector2Int ToAxis(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) return new Vector2Int((int)Mathf.Sign(v.x), 0);
        if (Mathf.Abs(v.y) > 0f) return new Vector2Int(0, (int)Mathf.Sign(v.y));
        return Vector2Int.zero;
    }

    // --------------------------------- sets positions :3
    void SetPost(Vector2 p)
    {
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }
    
    // --------------------------------- Cell stuff for the movement function + handles grid for tilemap (from pacstudent!)
    Vector2Int WorldToCell(Vector2 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    Vector2 CellToWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
    }
}

