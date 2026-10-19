using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource source;
    public AudioClip introMusic;
    public AudioClip loopMusic;
    
    private float waitTime;
    private float timer;
    private bool on = false;
    void Start()
    {
        source.clip = introMusic;
        source.loop = false;
        source.Play();
        
        waitTime = Mathf.Min(introMusic.length, 3f);
        timer = 0;
    }
    void Update()
    {
        if (on) return;
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            source.clip =  loopMusic;
            source.loop = true;
            source.Play();
            
            on = true;
        }
    }
}
