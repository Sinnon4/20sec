using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Player player;

    [SerializeField] public int gridSize = 1; //Need to adjust for room dimensions if altered


    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {SceneManager.LoadScene(0);}
    }
}
