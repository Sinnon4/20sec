using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreenPlayer : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] SpriteRenderer room1, room2;

    private void Awake()
    {
        GetComponent<Rigidbody2D>().linearVelocityY = speed;
    }

    private void Update()
    {
        if (transform.position.y > 2)
        {
            room1.color = Color.black;
            room2.color = Color.white;
        }

        if (transform.position.y > 10)
        {
            transform.position = new Vector2(transform.position.x, -20);
            room1.color = Color.white;
            room2.color = Color.black;
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
