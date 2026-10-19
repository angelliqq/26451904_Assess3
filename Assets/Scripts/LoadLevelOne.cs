using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelOne : MonoBehaviour
{

    public void LoadLevel()
    {
        SceneManager.LoadScene("Level1");
    }
}
