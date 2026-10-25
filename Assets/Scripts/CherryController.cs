using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CherryController : MonoBehaviour
{
    
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private Tilemap[] tilemaps;
    [SerializeField] private SpriteMask levelMask;

    private float delay = 5f;
    private float speed = 5f;
    private float padding = 1f;
    private int sorting = 10;

    private Bounds border;
    
    void Start()
    {
        border = GetLevelBounds();
        FitMask();
        StartCoroutine(CherryRoute());
    }

    // looping routine
    IEnumerator CherryRoute()
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            Vector2 start = RandomStart();
            Vector2 end = 2f * (Vector2)border.center - start;
            
            GameObject cherry = Instantiate(cherryPrefab, start, Quaternion.identity);
            Prepare(cherry);
            
            yield return MoveCherry(cherry, start, end);
            if (cherry == null) Destroy(cherry);
            
            yield return new WaitForSeconds(delay);
        }
    }

    // lerping - cherry movement
    IEnumerator MoveCherry(GameObject cherry, Vector2 start, Vector2 end)
    {
        float duration = Vector2.Distance(start, end) / speed;
        float elapsed = 0f;

        while (elapsed < duration && cherry != null)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            cherry.transform.position = Vector2.Lerp(start, end, time);
            yield return null;
        }
    }

    // random spawn location
    Vector2 RandomStart()
    {
        float left = border.min.x - padding;
        float right = border.max.x + padding;
        float up = border.max.y + padding;
        float down = border.min.y - padding;

        float randomX = Random.Range(border.min.x, border.max.x);
        float randomY = Random.Range(border.min.y, border.max.y);

        switch (Random.Range(0, 4))
        {
            case 0: return new Vector2(left, randomY);
            case 1: return new  Vector2(right, randomY);
            case 2: return new Vector2(randomX, down);
            default: return new Vector2(randomX, up);
        }
    }

    // prepares cherry
    void Prepare(GameObject cherry)
    {
        foreach (SpriteRenderer sr in cherry.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            sr.sortingOrder = sorting;
        }
    }

    Bounds GetLevelBounds()
    {
        Bounds b = new Bounds();
        bool first = true;

        foreach (Tilemap map in tilemaps)
        {
            if (map == null) continue;
            
            TilemapRenderer tr = map.GetComponent<TilemapRenderer>();
            if (tr == null) continue;

            if (first)
            {
                b = tr.bounds;
                first = false;
            }
            else
            {
                b.Encapsulate(tr.bounds);
            }
        }
        return b;
    }
    
    void FitMask()
    {
        if (levelMask == null || levelMask.sprite == null) return;
        Vector3 spriteSize = levelMask.sprite.bounds.size;
        
        levelMask.transform.position = new Vector3(border.center.x, border.center.y, levelMask.transform.position.z);
        
        levelMask.transform.localScale = new Vector3(border.size.x / spriteSize.x, border.size.y / spriteSize.y, 1f);
        
    }
}
