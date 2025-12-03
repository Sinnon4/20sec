using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreenPlayer : MonoBehaviour
{
    [SerializeField] float speed = 1;

    private void Awake()
    {
        GetComponent<Rigidbody2D>().linearVelocityY = -speed;
        GetComponent<Animator>().SetBool("downCheck", true);
    }

    private void Update()
    {
        if (transform.position.y < -10) transform.position = new Vector2(transform.position.x, 20);
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
