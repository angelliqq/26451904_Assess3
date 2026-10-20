using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitLevelOne : MonoBehaviour
{
      public void ExitLevel()
      {
       SceneManager.LoadScene("StartScene");
      }
}
