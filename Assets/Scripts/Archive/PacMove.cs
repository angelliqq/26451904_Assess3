using UnityEngine;

public class PacMove : MonoBehaviour
{
    public float speed = 3f;
    private Vector3[] path;
    private int index = 0;
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        Vector3 start =  transform.position;
        
        path = new Vector3[]
        {
            start + new Vector3(5, 0, 0),
            start + new Vector3(5, -4, 0),
            start + new Vector3(0, -4, 0),
            start,
        };
        
        FaceOtherDir();
    }
    
    void Update()
    {
        movingOnPath();
    }

    void movingOnPath()
    {
        Vector3 target  = path[index];
        Vector3 direction = (target - transform.position).normalized;
        
        transform.position += direction * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            index = (index + 1) % path.Length;
            FaceOtherDir();
        }
    }

    void FaceOtherDir()
    {
        Vector3 nextTarget = path[index];
        Vector3 dir = nextTarget - transform.position;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
                animator.Play("WalkRight");
            else 
                animator.Play("WalkLeft");
        }
        else 
        {
            if (dir.y > 0)
                animator.Play("WalkDown");
            else
                animator.Play("WalkUp");
        }
    }
    
}
