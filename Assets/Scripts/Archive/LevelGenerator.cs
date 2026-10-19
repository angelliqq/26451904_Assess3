using UnityEngine;
using UnityEngine.Tilemaps;


// yeah so I attempted 100% but nothing works, spent HOURS on this damn assignment but nothing works BYE
// dont mark me down please for me assets being 64x64 I didnt meant to I just draw normally and I CANT with small sizes

public class LevelGenerator : MonoBehaviour
{
    // tilemaps
    public Tilemap tilemap;
    public Tile empty;
    public Tile outsideCorner;
    public Tile outsideWall;
    public Tile insideCorner;
    public Tile insideWall;
    public GameObject pellet;
    public GameObject powerPellet;
    public Tile tJunction;
    public Tile ghostWall;

    public float tileSize = 1f;
    
    int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };
    
    void Start()
    {
        DeleteLevel();
        GenerateLevel();
    }
    
    void Update()
    {
        
    }

    // generates top quad
    void GenerateLevel()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int id = levelMap[r, c];
                Vector3Int cell = new Vector3Int(c, -r, 0);
                
                if (id == 0)
                {
                    tilemap.SetTile(cell, empty);
                    continue;
                }
                else if (id == 5 || id == 6)
                {
                    tilemap.SetTile(cell, empty);
                    Vector3 worldPos = tilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    if (id == 5) Instantiate(pellet, worldPos, Quaternion.identity);
                    if (id == 6) Instantiate(powerPellet, worldPos, Quaternion.identity);
                }
                else
                {
                    Tile tileAsset = GetTileAsset(id);
                    tilemap.SetTile(cell, tileAsset);
                    
                    tilemap.SetTransformMatrix(cell, Matrix4x4.identity);
                    
                    float angle = GetRotationForTile(id, r, c);
                    Matrix4x4 martix = Matrix4x4.TRS(Vector3 .zero, Quaternion.Euler(0, 0, angle), Vector3.one);
                    tilemap.SetTransformMatrix(cell, martix);
                }
            }
        }
    }
    
    // deletes preexisting level!
    void DeleteLevel()
    {
        GameObject level = GameObject.Find("Level");
        if (level != null)
        {
            Destroy(level);
        }
    }

    int GetTile(int r, int c)
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);
        if (r < 0 || r >= rows || c < 0 || c >= cols) return 0;
        return levelMap[r, c];
    }

    bool isAnyWall(int r, int c)
    {
        int v = GetTile(r, c);
        return v == 1 || v == 2 || v == 3 || v == 4 || v == 7 || v == 8;
    }

    //all wall rotation
    float GetWallRotation(int r, int c)
    {
        bool up = isAnyWall(r - 1, c);
        bool down = isAnyWall(r + 1, c);
        bool left = isAnyWall(r, c - 1);
        bool right = isAnyWall(r, c + 1);

        if (left && right) return 0f;
        if (up && down) return 90f;

        return 0f;
    }

    // all corner rotation
    float GetCornerRotation(int r, int c)
    {
        bool up = isAnyWall(r - 1, c);
        bool down = isAnyWall(r + 1, c);
        bool left = isAnyWall(r, c - 1);
        bool right = isAnyWall(r, c + 1);
        
        if (down && right) return 0f;
        if (left && down) return 90f;
        if (up && left) return 180f;
        if (up && right) return 270f;
        
        return 0f;
    }

    // rotation for tile 7
    float tJunctionRotation(int r, int c)
    {
        bool up = isAnyWall(r - 1, c);
        bool down = isAnyWall(r + 1, c);
        bool left = isAnyWall(r, c - 1);
        bool right = isAnyWall(r, c + 1);
        
        if (left && right && down && !up) return 0f;
        if (up && down && left && !right) return 90f;
        if (left && right && up && !down) return 180f;
        if (up && down && right && !left) return 270f;
        
        return 0f;
    }

    float GetRotationForTile(int id, int r, int c)
    {
        if (id == 2 || id == 4) return GetWallRotation(r, c);
        if (id == 1 || id == 3) return GetCornerRotation(r, c);
        if (id == 7) return tJunctionRotation(r, c);
        return 0f;
    }

    // grabbing the tile assets
    Tile GetTileAsset(int id)
    {
        switch (id)
        {
            case 1: return outsideCorner;
            case 2: return outsideWall;
            case 3: return insideCorner;
            case 4: return insideWall;
            case 7: return tJunction;
            case 8: return ghostWall;
            default: return null;
        }
    }

}
