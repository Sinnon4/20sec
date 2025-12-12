using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreen : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1); //goes to loading screen
    }
}
