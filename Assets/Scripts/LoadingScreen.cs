using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] float duration = 2, endX;
    float y;

    private void Start()
    {
        y = transform.position.y;
        StartCoroutine(LoadGame());
    }

    private void Update()
    {
        transform.position = new Vector2(transform.position.x, Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI * 2 / (duration * 2)) + y);
    }

    IEnumerator LoadGame()
    {
        transform.LeanMoveX(endX, duration);
        yield return new WaitForSeconds(duration);

        SceneManager.LoadSceneAsync(2);
    }
}
