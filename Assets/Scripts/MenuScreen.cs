using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreen : MonoBehaviour
{
    [SerializeField] GameObject buttons, infoScreen;

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1); //goes to loading screen
    }

    public void info()
    {
        buttons.SetActive(false);
        infoScreen.SetActive(true);
    }

    public void Back()
    {
        buttons.SetActive(true);
        infoScreen.SetActive(false);
    }
}
