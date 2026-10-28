using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostController : MonoBehaviour
{

    [SerializeField] private int ghostNumber = 1;
    
    [SerializeField] private Tilemap[] tileMap;
    [SerializeField] private Transform tpLeft;
    [SerializeField] private Transform tpRight;

    [SerializeField] private PacStudentController pacStudent;
    [SerializeField] private GhostManager ghostManager;

    [SerializeField] private Transform[] exitPath;
    [SerializeField] private Transform[] route;
    
    
    enum Mode
    {
        Spawn,
        Roam,
        Dead,
    }
    
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }


}
